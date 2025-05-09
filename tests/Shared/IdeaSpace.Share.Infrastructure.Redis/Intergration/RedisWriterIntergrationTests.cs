using Xunit;
using StackExchange.Redis;
using Testcontainers.Redis;
using System.Text.Json;
using Shared.Infrastructure.Redis.Interface.Core;
using Shared.Infrastructure.Redis.Core.Write.Extend;
using Moq;
using Serilog;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Redis.Core;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Redis.Model.Config;
using Shared.Infrastructure.Redis.Interface.Core.Redis;
using Shared.Infrastructure.Redis.Config;

namespace IdeaSpace.Share.Infrastructure.Redis.Intergration
{
    public class MockUserKey : IRedisSerialise
    {
        public string UserKey { get; set; }
        public RedisKey ToRedisKey()
        {
            return new RedisKey($"Test:{UserKey}");
        }
    }
    public class MockUserValue
    {
        public string Value { get; set; }
    }
    public class RedisWriterIntergrationTests : IAsyncLifetime
    {
        private readonly RedisContainer _container;
        private IRedisConnectionManger _conManager;
        private IWrite<MockUserKey, MockUserValue> _writer;
        private int _escalationCall = 0;
        public RedisWriterIntergrationTests()
        {
            _container = new RedisBuilder()
                .WithImage("redis:7.2.4")
                .WithCleanUp(true)
                .WithName("redis-test-{Guid.NewGuid()}")
                .WithPortBinding(6379, true)
                .Build();
        }

        
        public async Task DisposeAsync()
        {
            await _conManager.CloseAsync();
            await _container.StopAsync();
        }

        public async Task InitializeAsync()
        {
            // start the test container
            await _container.StartAsync();

            var mockLogger = new Mock<ILogger>();
            var mockConfig = new Mock<IConfiguration>();
            IOptions<RedisOptions> options = Options.Create(new RedisOptions
            {
                ConnectionString = _container.GetConnectionString() // or your custom value
            });
            var moduleData = new ModuleMetaData
            {
                IssuerType = Shared.Infrastructure.Observability.IssuerType.Internal,
                IssuerId = Guid.NewGuid(),
                RefillRate = 100,
                Ttl = 5000
            };

            _conManager = new RedisConnectionManager(mockLogger.Object, options,mockConfig.Object, moduleData);
            _writer = new RedisWriterOutBox<MockUserKey, MockUserValue>(
                _conManager,
                new JsonSerializerOptions(),
                mockLogger.Object,
                mockConfig.Object,
                () => { _escalationCall++; return Task.CompletedTask; }
                );
        }

    }
}
