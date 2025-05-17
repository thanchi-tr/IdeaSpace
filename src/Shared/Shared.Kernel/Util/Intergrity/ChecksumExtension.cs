using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Shared.Kernel.Util.Intergrity
{
    public static class ChecksumExtension
    {
        public static string ComputeChecksum<T>(this T str)
        {
            // convert to stabel byte
            var json = JsonSerializer.Serialize<T>(str, JsonSerializerStableOptions.Checksum);
            var bytes = Encoding.UTF8.GetBytes(json);

            using var sha = SHA512.Create();
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
