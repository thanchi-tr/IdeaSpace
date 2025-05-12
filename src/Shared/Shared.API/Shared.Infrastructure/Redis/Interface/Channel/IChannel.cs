using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Redis.Interface.Channel
{
    public interface IChannel<T, KeyDTO, ValueDTO>
    {
        ChannelWriter<T> Writer { get; }
        ChannelReader<T> Reader { get; }
    }
}
