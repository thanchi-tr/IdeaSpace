namespace Shared.Kernel.Observability.Logging.Constant
{
    public sealed record ServiceType(string Name)
    {
        public static readonly ServiceType Redis = new("Redis");
        public static readonly ServiceType RabbitMQConsumer = new("RabbitMQ_Consumer");
        public static readonly ServiceType RabbitMQPublisher = new("RabbitMQ_Publisher");
        public static readonly ServiceType InternalCall = new("InternalService");
        public static readonly ServiceType SelfRecover = new("SelfRecover");
        public static readonly ServiceType Logging = new("Logging");
        

        public override string ToString() => Name;
    }
}
