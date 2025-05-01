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
        public const string RECOVER_DLQ = "sys.recover.dead.letter.queue";

        public const string CACHING_OPERATION = "cache.operate.lazy.queue";
        public const string CACHING_OPERATION_DLQ = "cache.operate.lazy.dead.letter.queue";
        public const string MODIFY_OPERATION = "modify.operate.lazy.queue";
        public const string MODIFY_OPERATION_DLQ = "modify.operate.lazy.dead.letter.queue";
        public const string CREATE_OPERATION = "create.operate.lazy.queue";
        public const string CREATE_OPERATION_DLQ = "create.operate.lazy.dead.letter.queue";

        public const string DELTA_LOG = "sys.recover.delta.queue";
        public const string DELTA_LOG_DLQ = "sys.recover.delta.dead.letter.queue";
        public const string RECOVER_COMMAND = "sys.recover.command.queue";
        public const string RECOVER_COMMAND_DLQ = "sys.recover.command.dead.letter.queue";
        public const string RECOVER_CLEAN = "sys.recover.clean.queue";
        public const string RECOVER_CLEAN_DLQ = "sys.recover.clean.dead.letter.queue";

        public const string VIEW_OPERATION = "view.operate.eager.queue";
        public const string VIEW_OPERATION_DLQ = "view.operate.eager.dead.letter.queue";
        public const string VALIDATE_OPERATION = "validate.operate.eager.queue";
        public const string VALIDATE_OPERATION_DLQ = "validate.operate.eager.dead.letter.queue";

        public const string MEDIATOR_MODIFY_REQ = "modify.request.result.queue";
        public const string MEDIATOR_MODIFY_REQ_DLQ = "modify.request.result.dead.letter.queue";
        public const string MEDIATOR_GET_REQ = "get.request.result.queue";
        public const string MEDIATOR_GET_REQ_DLQ = "get.request.result.dead.letter.queue";
        public const string MEDIATOR_CLEAN = "clean.request.result.queue";
        public const string MEDIATOR_CLEAN_DLQ = "clean.request.result.dead.letter.queue";
    }
}
