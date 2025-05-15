
using APIGateway.API.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using Shared.Kernel.GeneralConfig;
using Shared.Messaging.Interface.Contract;
using System.Net;
using System.Text;

namespace APIGateway.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            int BITS256_IN_NUM = 32;
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var config = builder.Configuration;
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Listen(IPAddress.Any, 80); // redirect only
                serverOptions.Listen(IPAddress.Any, 443, listenOptions =>
                {
                    listenOptions.UseHttps("certs/devcert.pfx", "testpass");
                });
            });
            builder.Services.Configure<HstsOptions>(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
                options.Preload = true;
            });
            builder.Services.ConfigHttpClient(config); // set up all the client http config and polly
            builder.Services.Configure<RabbitMqOptions>(config.GetSection("RabbitMQ"));
            // Add YARP reverse proxy
            builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // Register Defaul Identity Validation parameter use with Jwt validator
            builder.Services.AddSingleton<TokenValidationParameters>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var secret = configuration["Jwt:SecretKey"];//?

                // This is in pre build, we want it to fail fast
                if (string.IsNullOrWhiteSpace(secret) || secret.Length < BITS256_IN_NUM)
                    throw new InvalidOperationException("JWT secret must be at least 256 bits (32 characters)");

                return new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

            builder.Services.KernelConfigExtension(builder.Configuration);
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseHsts();

            app.MapReverseProxy();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
