using Microsoft.Extensions.Configuration;

namespace Shared.Infrastructure.Observability
{
    public static class ExtractTraceIdFromConfig
    {
        /// <summary>
        /// Expect to be reuse accors module extensively,
        /// Only time where this fail is of missing config,
        /// we expect config to be sound, and expect application cant
        /// run with stale config.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static TraceId ExtractTraceId(this IConfiguration config, string moduleName)
        {
            var section = config.GetSection($"AppMetaData:ModulesMDatas:{moduleName}");
            if (!section.Exists())
                throw new InvalidOperationException($"Missing TraceId config for module {moduleName}");

            var id = Guid.Parse(section["IssuerId"]!);
            var type = int.Parse(section["IssuerType"]!);

            return new TraceId(id, type);
        }

    }
}
