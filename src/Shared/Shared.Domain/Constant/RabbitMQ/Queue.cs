using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Domain.Constant.RabbitMQ
{
    public static class Queue
    {
        public const string HEALTH_CHECK = "check.health.queue";
        public const string RECOVER = "sys.recover";

        public const string CACHING_OPERATION = "cache.operate.lazy";
        public const string MODIFY_OPERATION = "modify.operate.lazy";
        public const string CREATE_OPERATION = "create.operate.lazy";

        public const string DELTA_LOG = "sys.recover.delta";
        public const string RECOVER_COMMAND = "sys.recover.command";
        public const string RECOVER_CLEAN = "sys.recover.clean";

        public const string VIEW_OPERATION = "view.operate.eager";
        public const string VALIDATE_OPERATION = "validate.operate.eager";

        public const string MEDIATOR_MODIFY_REQ = "modify.request.result";
        public const string MEDIATOR_GET_REQ = "get.request.result";
        public const string MEDIATOR_CLEAN = "clean.request.result";
    }
}
