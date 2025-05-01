
namespace Shared.Messaging.Interface.RabbitMqSettings
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; } = "localhost";
        public string UserName { get; set; } = "admin";
        public string Password { get; set; } = "admin";
        public string Port { get; set; } = "5672";
    }
}
