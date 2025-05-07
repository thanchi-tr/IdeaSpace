namespace Shared.Kernel.Util.Jwt
{
    public static class ParseTTL
    {
        public static bool TryParseTtl(string ttlStr, out TimeSpan? ttl)
        {
            ttl = null;

            if (!int.TryParse(ttlStr, out var ttlSeconds))
            {
                return false;
            }

            ttl = TimeSpan.FromSeconds(ttlSeconds);
            return true;
        }

    }
}
