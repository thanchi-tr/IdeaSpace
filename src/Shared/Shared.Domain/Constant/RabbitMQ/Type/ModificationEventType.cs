using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Domain.Constant.RabbitMQ.Type
{
    public enum ModificationEventType
    {
        Upsert = 0,
        Delete = 1,
    }
}
