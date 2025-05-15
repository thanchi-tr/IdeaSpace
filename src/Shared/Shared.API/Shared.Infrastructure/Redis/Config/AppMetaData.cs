using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Redis.Config
{
    public class AppMetaData
    {
        public Dictionary<string, ModuleMetaData> ModulesMDatas { get; set; } = new();
    }
}
