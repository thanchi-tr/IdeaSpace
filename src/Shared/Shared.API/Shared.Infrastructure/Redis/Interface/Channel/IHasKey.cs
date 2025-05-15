using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Redis.Interface.Channel
{
    public interface IHasKey<TKey>
    {
        TKey Key { get; }
    }

}
