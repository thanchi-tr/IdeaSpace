using APIGateway.API.Interface.Middleware;
using APIGateway.API.Model.DTO;
using Microsoft.IdentityModel.Tokens;
using Serilog.Context;
using Shared.Infrastructure.Observability;
using Shared.Infrastructure.Redis.Interface.Core.Redis;
using Shared.Infrastructure.Redis.Model.DTO.Key;
using Shared.Kernel.Observability.Logging;
using Shared.Kernel.Observability.Logging.Constant;
using Shared.Kernel.Util.Jwt;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;

namespace APIGateway.API.Service.Auth
{
    public class JwtValidator : IJwtValidator
    {
        private IRead<RedisWLJWTKey,UserContext> _whiteListReader;
        private IWrite<RedisWLJWTKey, UserContext> _whiteListWriter;

        private IRead<RedisBLJWTKey, string> _blackListReader;
        private Serilog.ILogger _auditLogger;
        private JwtSecurityTokenHandler _builtInIwtHandler;
        private TokenValidationParameters _jtParam;
        private TraceId _clientTraceId;
        public JwtValidator(
            IWrite<RedisWLJWTKey, UserContext> whiteListWriter,
            IRead<RedisWLJWTKey, UserContext> whiteListReader,
            IRead<RedisBLJWTKey, string> blackListReader,
            Serilog.ILogger baseLogger, 
            TokenValidationParameters jtParam,
            TraceId clientTraceId)
        {
            _whiteListReader = whiteListReader;
            _whiteListWriter = whiteListWriter;
            _blackListReader = blackListReader;
            _auditLogger = baseLogger.Split()[LoggerType.AuditLog];
            _jtParam = jtParam;
            _builtInIwtHandler = new JwtSecurityTokenHandler();
            _clientTraceId = clientTraceId;
        }

        private Dictionary<string, string>? TryExtractClaims(string token)
        {
            try
            {
                var principal = _builtInIwtHandler.ValidateToken(token, _jtParam, out _);
                return principal.Claims.ToDictionary(c => c.Type, c => c.Value);
            }
            catch (SecurityTokenException ex)
            {
                using (LogContext.PushProperty("TraceId", _clientTraceId))
                {
                    _auditLogger.Warning("JWT validation failed", ex);
                }
                return null;
            }
        }

        public async Task<UserContext?> ValidateAsync(string token)
        {
            var claims = TryExtractClaims(token);
            if (claims == null || !ClaimsHelper.HasAllRequiredClaims(claims, required: ["jti", "sub", "email", "name", "role", "ttl"]))
                return null;

            // subsequence log in 
            var JwtKey = new RedisWLJWTKey { Jti = claims["jti"] };
            var context = await _whiteListReader.ReadAsync(JwtKey);
            if(context != null) {
                using (LogContext.PushProperty("TraceId", _clientTraceId))
                {
                    _auditLogger.Information("User {UserName} relogin successful at {Time}", context.UserName, DateTime.UtcNow);
                }
                return context;
            }
            // check black list token
            if(await _blackListReader.ReadAsync(new RedisBLJWTKey { Jti = claims["jti"] }) != null)
            {
                using (LogContext.PushProperty("TraceId", _clientTraceId))
                {
                    // this one is sufficient for now, this can easily extended into full fetch restricted account by keep count
                    // and once hit max allow, account is softban
                    _auditLogger.Warning("User login rejected at {Time} using Blacklisted Token.", DateTime.UtcNow);
                }
                return null;
            }
            // check with redis if user is in white list (match jwt) // implement later when reach Redis 
            // right now assume that it does exist in whitelist
            

            context = BuildUserContext(claims);
            if (context == null)
                return null;

            if (!ParseTTL.TryParseTtl(claims["ttl"], out var ttl) || ttl == null)
                return null;
            using (LogContext.PushProperty("TraceId", _clientTraceId))
            {
                _auditLogger.Information("User {UserName} login successful at {Time}", context.UserName, DateTime.UtcNow);
            }
            await _whiteListWriter.WriteAsync(JwtKey, context, ttl.Value);
            return context;
        }

        public UserContext? BuildUserContext(Dictionary<string, string> claims)
        {
            if (!Guid.TryParse(claims["sub"], out var id)) return null;
            return new UserContext
            {
                UserId = id,
                Email = claims["email"],
                UserName = claims["name"],
                AuthenticatedAt = DateTime.UtcNow,
                Roles = claims.Where(c => c.Key == "role").Select(c => c.Value).ToList()
            };
        }

    }
}
