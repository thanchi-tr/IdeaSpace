using APIGateway.API.Interface.Middleware;
using APIGateway.API.Model.DTO;
using Microsoft.IdentityModel.Tokens;
using Serilog.Context;
using Shared.Infrastructure.Observability;
using Shared.Infrastructure.Redis.Interface.Core;
using Shared.Kernel.Observability.Logging;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;

namespace APIGateway.API.Service.Auth
{
    public class JwtValidator : IJwtValidator
    {
        private IConnectionMultiplexer _redis;
        private Dictionary<string, Serilog.ILogger> _loggers;
        private JwtSecurityTokenHandler _builtInIwtHandler;
        private TokenValidationParameters _jtParam;

        public JwtValidator(
            IRedisConnectionManger _redisManager, 
            Serilog.ILogger baseLogger, 
            TokenValidationParameters jtParam)
        {
            _redis = _redisManager.GetConnection();
            _loggers = baseLogger.Split();
            _jtParam = jtParam;
            _builtInIwtHandler = new JwtSecurityTokenHandler();
        }

        public Dictionary<string, string> ExtractClaims(string token)
        {
            new Dictionary<string, string>();
            var principal = _builtInIwtHandler.ValidateToken(token,_jtParam, out _);
            return principal.Claims.ToDictionary(claim => claim.Type, claim => claim.Value);
        }

        public UserContext? Validate(string token)
        {
            var claims = ExtractClaims(token);
            // check with redis if user is in white list (match jwt) // implement later when reach Redis 
            // right now assume that it does exist in whitelist
            if(claims == null )
            {
                return null;
            }

            if(!Guid.TryParse(claims["sub"], out var userid))
            {
                using (LogContext.PushProperty(
                    "TraceId",
                    new TraceId
                    {
                        IssuerType = IssuerType.Client,
                        IssuerId = Guid.NewGuid(), // later on will swap this for the 
                        Timestamp = DateTime.UtcNow,
                    }))
                {

                    _loggers[LoggerType.AuditLog].Warning($"missing userId");
                }
                return null;
            }
            if (!claims.ContainsKey("email") ||
                !claims.ContainsKey("name") ||
                !claims.ContainsKey("role")) {
                // User's jwt failure should be audit event, where
                // human agent want to trace client logging issue
                using (LogContext.PushProperty(
                    "TraceId",
                    new TraceId
                    {
                        IssuerType = IssuerType.Client,
                        IssuerId = userid, // later on will swap this for the 
                        Timestamp = DateTime.UtcNow,
                    }

                    ))
                {

                    _loggers[LoggerType.AuditLog].Warning("Invalid Jwt");
                }
                return null;
            }

            return new UserContext
            {
                UserId = userid,
                Email = claims["email"],
                UserName = claims["name"],
                AuthenticatedAt = DateTime.UtcNow, // dummy for now, this should be also extract from redis
                Roles = claims
                    .Where(c => c.Key == "role")
                    .Select(c => c.Value)
                    .ToList()
            };
        }
    }
}
