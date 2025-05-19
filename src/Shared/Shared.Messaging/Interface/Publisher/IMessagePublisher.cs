using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messaging.Interface.Publisher
{
    public interface IMessagePublisher<T>
    {
        Task PublishAsync(T message, CancellationToken ct);
    }
}
