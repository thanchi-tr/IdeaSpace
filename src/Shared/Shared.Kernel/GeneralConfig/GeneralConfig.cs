using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Kernel.Observability.HealthCheck;
using System.Text;

namespace Shared.Kernel.GeneralConfig
{
    public static  class KernelExtension
    {
        /// <summary>
        /// This extension allow for config base service (e.g HealthCheck)
        /// This a base contract for all module to follow
        ///     1) all of them can rest asure that Mq is healthy (bse method for intermodule comm)
        ///     2) block external access, only via API gateway, internally (from other module) opt 
        ///     for MQ event.
        /// Make sure the following exist in your setting
        /// "AllowedOrigins": [
        ///"https://localhost:5003", // list all the port use by API gateway or default to 5003
        ///"https://staging.yourapp.com"

        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection KernelConfigExtension (this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("AllowedOrigins")
                    .Get<string[]>() ?? new[] { "https://localhost:5003" };

            
            services.AddCors(options =>
            {
                options.AddPolicy("InternalAllow", policy =>
                {
                    policy.WithOrigins(allowedOrigins) // Allow your frontend origin
                          .AllowAnyMethod() // Allow any HTTP method (GET, POST, etc.)
                          .AllowAnyHeader() // Allow any headers (e.g., Content-Type, Authorization)
                          .AllowCredentials(); // If you need cookies or authorization headers
                });
            });
            return services;
        }
    }
}
