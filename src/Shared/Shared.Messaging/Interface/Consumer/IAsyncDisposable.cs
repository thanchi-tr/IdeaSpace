using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messaging.Interface.Consumer
{
    public interface IAsyncDisposable
    {
        ValueTask DisposeAsync();
    }
}
