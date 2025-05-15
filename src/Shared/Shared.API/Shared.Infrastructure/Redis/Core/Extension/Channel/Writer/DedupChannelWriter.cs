using Shared.Infrastructure.Redis.Interface.Channel;
using System.Threading.Channels;

namespace Shared.Infrastructure.Redis.Core.Extension.Channel.Writer
{
    public class DedupChannelWriter<T, KeyDTO, ValueDTO> : ChannelWriter<T>
        where T : class, IHasKey<KeyDTO>, IHaveSetValue<ValueDTO>
        where KeyDTO: notnull
    {
        private readonly DeduplicatedChannel<T, KeyDTO, ValueDTO> _outer;

        public DedupChannelWriter(DeduplicatedChannel<T, KeyDTO, ValueDTO> outer) => _outer = outer;

        public override bool TryWrite(T item)
        {
            if (_outer._inFlight.TryAdd(item.Key, item))
                return _outer._channel.Writer.TryWrite(item);

            if (_outer._inFlight.TryGetValue(item.Key, out var existing))
            {
                // assume mutation-safe
                existing.Value = item.Value;
                return false;
            }

            return true;
        }

        public override async ValueTask WriteAsync(T item, CancellationToken cancellationToken = default)
        {
            if (_outer._inFlight.TryAdd(item.Key, item))
            {
                await _outer._channel.Writer.WriteAsync(item, cancellationToken);
            }
            else if (_outer._inFlight.TryGetValue(item.Key, out var existing))
            {
                existing.Value = item.Value;
            }
        }

        public override bool TryComplete(Exception error )
            => _outer._channel.Writer.TryComplete(error);

        public override ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = default)
        {
            //throw new NotImplementedException();
            return ValueTask.FromResult(false);// do nothing
        }
    }

}
