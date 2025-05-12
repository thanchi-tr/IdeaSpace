using Shared.Infrastructure.Redis.Core.Extension.Channel.Reader;
using Shared.Infrastructure.Redis.Core.Extension.Channel.Writer;
using Shared.Infrastructure.Redis.Interface.Channel;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Shared.Infrastructure.Redis.Core.Extension.Channel
{

    public class DeduplicatedChannel<T, KeyDTO, ValueDTO>  : IChannel<T, KeyDTO, ValueDTO>
        where T : class, IHasKey<KeyDTO>, IHaveSetValue<ValueDTO>
        where KeyDTO: notnull
    {
        internal readonly Channel<T> _channel;
        internal readonly ConcurrentDictionary<KeyDTO, T> _inFlight = new();

        public DeduplicatedChannel(Channel<T> channel)
        {
            _channel = channel;
        }

        public ChannelWriter<T> Writer => new DedupChannelWriter<T, KeyDTO, ValueDTO>(this);
        public ChannelReader<T> Reader => new CleanupChannelReader<T, KeyDTO, ValueDTO>(this);
    }

}
