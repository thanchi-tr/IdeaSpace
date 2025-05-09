using Xunit;
using StackExchange.Redis;
using Testcontainers.Redis;
using FluentAssertions;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using Shared.Infrastructure.Redis.Core.Write;
using Shared.Infrastructure.Redis.Interface.Core;
using Shared.Infrastructure.Redis.Core.Write.Extend;
using Moq;
using Serilog;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Redis.Core;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Redis.Model.Config;

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
        private RedisWriter<MockUserKey, MockUserValue> _writer;
        
        public RedisWriterIntergrationTests()
        {
            _container = new RedisBuilder()
                .WithImage("redis:7")
                .WithCleanUp(true)
                .WithName("redis-test")
                .WithPortBinding(6379, true)
                .Build();
        }

        
        public async Task DisposeAsync()
        {
            await _conManager.Dispose();
            await _container.StopAsync();
        }

        public async Task InitializeAsync()
        {
            // start the test container
            await _container.StartAsync();

            var mockLogger = new Mock<ILogger>();
            var mockConfig = new Mock<IConfiguration>();
            // Arrange
            //var options = Options.Create(new RedisOptions
            //{
            //    ConnectionString = "localhost:6379", // or from testcontainers 
            //});
            //_conManager = new RedisConnectionManager(
            //    mockLogger.Object,
            //    options,
            //    mockConfig.Object
            //    );
            //_writer = new RedisWriterOutBox<MockUserKey, MockUserValue>(
            //    _conManager, 
            //    new JsonSerializerOptions(),
            //    mockLogger.Object,
            //    mockConfig.Object,
            //    () => { return Task.CompletedTask; }
            //);
        }

    }
}
