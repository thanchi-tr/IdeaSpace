using System.Text.Json.Serialization;
using System.Text.Json;

namespace Shared.Kernel.Util.Intergrity
{
    public static class JsonSerializerStableOptions
    {
        public static JsonSerializerOptions Object => new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static JsonSerializerOptions Checksum => new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
        };
    }
}
