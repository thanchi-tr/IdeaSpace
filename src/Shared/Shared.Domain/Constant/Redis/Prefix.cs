using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Domain.Constant.Redis
{
    public static class Prefix
    {
        public const string JwtBlackList = "HardDeny";
        public const string JwtWhiteList = "UserWhiteList";
        public const string WriteDomainData = "GateKeeper";
        public const string ReadKeyDomainData = "ExpirationDisplay";
        public const string HealthCheck = "HealthCheck";
    }
}
