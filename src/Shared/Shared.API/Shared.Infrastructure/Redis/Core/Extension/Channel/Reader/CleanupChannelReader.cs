using Shared.Infrastructure.Redis.Interface.Channel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Redis.Core.Extension.Channel.Reader
{
    public class CleanupChannelReader<T, KeyDTO, ValueDTO> : ChannelReader<T>
        where T : class, IHasKey<KeyDTO>, IHaveSetValue<ValueDTO>
        where KeyDTO : notnull
    {
        private readonly DeduplicatedChannel<T, KeyDTO, ValueDTO> _outer;

        public CleanupChannelReader(DeduplicatedChannel<T, KeyDTO, ValueDTO> outer) => _outer = outer;

        public override async ValueTask<T> ReadAsync(CancellationToken cancellationToken = default)
        {
            var item = await _outer._channel.Reader.ReadAsync(cancellationToken);
            _outer._inFlight.TryRemove(item.Key, out _);
            return item;
        }

        public override bool TryRead(out T item)
        {
            var success = _outer._channel.Reader.TryRead(out item);
            if (success)
                _outer._inFlight.TryRemove(item.Key, out _);
            return success;
        }

        public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
            => _outer._channel.Reader.WaitToReadAsync(cancellationToken);

    }

}
