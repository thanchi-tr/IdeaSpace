using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Kernel.Util.Jwt
{
    public static class ClaimsHelper
    {
        public static bool HasAllRequiredClaims(Dictionary<string, string> claims, params string[] required)
            => required.All(k => claims.ContainsKey(k));


    }
}
