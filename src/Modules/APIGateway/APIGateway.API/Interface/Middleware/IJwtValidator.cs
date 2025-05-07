using APIGateway.API.Model.DTO;
using Microsoft.Extensions.Options;

namespace APIGateway.API.Interface.Middleware
{
    public interface IJwtValidator
    {
        /// <summary>
        /// Validate the token
        ///     - valid ?? parse claim into UserContext
        ///     - invalid ?? return null
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        UserContext? Validate(string token);
        
        /// <summary>
        /// Intended usage:
        ///     - in the jwt, parse and map all claim to Dictionary
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Dictionary<string, string> ExtractClaims(string token);
    }
}
