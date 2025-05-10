using Testcontainers.Redis;
using System.Text.Json;
using Shared.Infrastructure.Redis.Core.Write.Extend;
using Serilog;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Redis.Core;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Redis.Model.Config;
using Shared.Infrastructure.Redis.Config;
using IdeaSpace.Share.Infrastructure.Redis.Intergration.Model;
using Shared.Infrastructure.Redis.Interface.Core;
using System.Reflection;
using Shared.Infrastructure.Redis.Interface.Extension.Recovery;
using Serilog.Sinks.TestCorrelator;

namespace IdeaSpace.Share.Infrastructure.Redis.Intergration
{
    
   
    public class RedisWriterIntergrationTests : IAsyncLifetime
    {
        private readonly RedisContainer _container;
        private IRedisConnectionManger _conManager;
        private IRedisWriteOutBox<MockUserKey, MockUserValue> _writer;
        private int _escalationCall = 0;
        private Serilog.Core.Logger mockLogger;
        public RedisWriterIntergrationTests()
        {
            _container = new RedisBuilder()
                .WithImage("redis:7.2.4")
                .WithCleanUp(true)
                .WithName($"redis-test-{Guid.NewGuid()}")
                .WithPortBinding(6379, true)
                .Build();
        }

        
        public async Task DisposeAsync()
        {
            await _conManager.CloseAsync();
            if(_container != null) await _container.StopAsync();
        }

        public async Task InitializeAsync()
        {
            // start the test container
            await _container.StartAsync();

            mockLogger = new LoggerConfiguration()
                .WriteTo.TestCorrelator()
                .Enrich.FromLogContext()
                .CreateLogger();

            Log.Logger = mockLogger;
            var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory()) // or specify your test project path
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .Build();
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

            _conManager = new RedisConnectionManager(mockLogger, options, configuration, moduleData);
            _writer = new RedisWriterOutBox<MockUserKey, MockUserValue>(
                _conManager,
                new JsonSerializerOptions(),
                mockLogger,
                configuration,
                () => { 
                    _escalationCall+=1; 
                    return Task.CompletedTask; }
                );
        }


        [Fact]
        public async Task RedisWriteShouldBeReadable()
        {
            // Arrange
            var userKey = new MockUserKey
            {
                UserKey = "key"
            };
            var searchKey = new MockUserKey
            {
                UserKey = "key"
            };
            var userValue = new MockUserValue
            {
                Value = "message"
            };
            var mockTtl = 1000;
            var openConnection = _conManager.GetDatabase(0); // probably same db
            var ct = new CancellationTokenSource();
            // Act
            using (TestCorrelator.CreateContext())
            {

                await _writer.EnqueueAsync(userKey, userValue, TimeSpan.FromSeconds(mockTtl), ct.Token);
                var read = await openConnection.StringGetAsync(searchKey.ToRedisKey());
                var events = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
                // Assert
                Assert.True(events.Count == 0);
                Assert.True(read.HasValue);
                var value = JsonSerializer.Deserialize<MockUserValue>(read);
                Assert.Equal("message", value.Value);
            }

        }

        [Fact]
        public async Task RedisWriteShouldSelfHeal()
        {
            // Arrange
            var userKey = new MockUserKey
            {
                UserKey = "key"
            };
            var searchKey = new MockUserKey
            {
                UserKey = "key"
            };
            var userValue = new MockUserValue
            {
                Value = "message"
            };
            var mockTtl = 1000;
            var openConnection = _conManager.GetDatabase(-1); // probably same db
            var ct = new CancellationTokenSource();
            await _conManager.CloseAsync(); // ensure connection close
            var ttl = TimeSpan.FromSeconds(mockTtl);
            // Act
            using (TestCorrelator.CreateContext())
            {
               
                await _writer.EnqueueAsync(userKey, userValue, ttl, ct.Token);

                _conManager.AttemptHeal();
                openConnection = _conManager.GetDatabase(-1);
                var read = await openConnection.StringGetAsync(searchKey.ToRedisKey());
                var events = TestCorrelator.GetLogEventsFromCurrentContext().ToList();

                var moduleName = Assembly.GetEntryAssembly()?.GetName().Name ?? "";
                // Assert
                Assert.True(events.Count > 0);
                Assert.True(read.HasValue);
                var value = JsonSerializer.Deserialize<MockUserValue>(read);
                Assert.Equal("message", value.Value);
            }
        }

        [Fact]
        public async Task RedisWriteShouldFailIfContainerDrop()
        {
            // Arrange
            var userKey = new MockUserKey
            {
                UserKey = "key"
            };
            var searchKey = new MockUserKey
            {
                UserKey = "key"
            };
            var userValue = new MockUserValue
            {
                Value = "message"
            };
            var mockTtl = 1000;
            var openConnection = _conManager.GetDatabase(-1); // probably same db
            var ct = new CancellationTokenSource();
            await _container.StopAsync();
            var ttl = TimeSpan.FromSeconds(mockTtl);
            // Act
            using (TestCorrelator.CreateContext())
            {
                await _writer.EnqueueAsync(userKey, userValue, ttl, ct.Token);
                var events = TestCorrelator.GetLogEventsFromCurrentContext().ToList();

                // Assert
                Assert.True(events.Count > 0);
                Assert.Equal(_escalationCall, 1);
            }
        }
    }
}
