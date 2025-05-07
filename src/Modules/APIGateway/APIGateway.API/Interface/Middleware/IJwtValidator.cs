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
        Task<UserContext?> ValidateAsync(string token);
    }
}
