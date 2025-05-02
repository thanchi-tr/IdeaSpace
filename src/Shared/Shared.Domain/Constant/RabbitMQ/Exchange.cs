namespace Shared.Domain.Constant.RabbitMQ
{
    public static class Exchange
    {
        public const string DLX = "sys.dead.letter.exchange";
        public const string HEALTH = "health";
        public const string RECOVER = "sys.recover";

        // use for define a eventual consistence event (asynchronous)
        public const string ASYNC_OPERATE = "sys.operate.lazy";
        // use for define a real time - or blocking event where response is expected
        public const string SYNC_OPERATE = "sys.operate.eager";
        // Client mediator
        public const string CLIENT_MEDIATOR = "request.result";
    }
}
