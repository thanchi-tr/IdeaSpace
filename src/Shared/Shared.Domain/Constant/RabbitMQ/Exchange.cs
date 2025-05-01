namespace Shared.Domain.Constant.RabbitMQ
{
    public static class Exchange
    {
        public const string HEALTH = "health";
        public const string RECOVER = "sys.recover";
        public const string RECOVER_DLX = "sys.recover.dlx";

        // use for define a eventual consistence event (asynchronous)
        public const string ASYNC_OPERATE = "sys.operate.lazy";
        public const string ASYNC_OPERATE_DLX = "sys.operate.lazy.dlx";
        // use for define a real time - or blocking event where response is expected
        public const string SYNC_OPERATE = "sys.operate.eager";
        public const string SYNC_OPERATE_DLX = "sys.operate.eager.dlx";
        // Client mediator
        public const string CLIENT_MEDIATOR = "request.result";
        public const string CLIENT_MEDIATOR_DLX = "request.result.dlx";
    }
}
