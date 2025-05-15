using IdeaSpace.Share.Infrastructure.Redis.Intergration.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Shared.Infrastructure.Redis.Core.Extension.Channel;
using Shared.Infrastructure.Redis.Core;
using Shared.Infrastructure.Redis.Interface.Channel;
using Shared.Infrastructure.Redis.Interface.Core;
using Shared.Infrastructure.Redis.Model.Config;
using Shared.Infrastructure.Redis.Model;
using System.Threading.Channels;
using Testcontainers.Redis;
using Shared.Infrastructure.Redis.Config;
using Shared.Infrastructure.Redis.Interface.Core.Redis;
using Shared.Infrastructure.Redis.Core.Read;
using Shared.Infrastructure.Redis.Interface.Extension.Recovery;
using Shared.Infrastructure.Observability;
using Shared.Infrastructure.Redis.Core.Write.Extend;
using System.Text.Json;

namespace IdeaSpace.Share.Infrastructure.Redis.Intergration
{
    public class RedisReaderIntergrationTest
    {
        private readonly RedisContainer _container;
        private Serilog.Core.Logger mockLogger;
        private IRedisConnectionManger _conManager;

        private IRedisConnectionManger _conReaderManager;
        private IRead<MockUserKey, MockUserValue> _reader;
        private IRedisWriteOutBox<MockUserKey, MockUserValue> _writerWithOutTageFlag;
        private TraceId _traceId = new TraceId(Guid.NewGuid(), IssuerType.Internal.GetHashCode());
        private int _escalationCall = 0;
        private int maxRetry;
        private int retryInterval;
        public RedisReaderIntergrationTest()
        {
            _container = new RedisBuilder()
                .WithImage("redis:7.2.4")
                .WithCleanUp(true)
                .WithName($"redis-test")
                .WithPortBinding(6379, true)
                .Build();
            
        }

        public async Task DisposeAsync()
        {
            await _conManager.CloseAsync();
            if (_container != null) await _container.StopAsync();
        }
        public async Task InitializeAsync()
        {
            // start the test container
            await _container.StartAsync();
            var services = new ServiceCollection();
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
            var mockOutboxChannel = new DeduplicatedChannel<RedisInstance<MockUserKey, MockUserValue>, MockUserKey, MockUserValue>(
            Channel.CreateBounded<RedisInstance<MockUserKey, MockUserValue>>(
                new BoundedChannelOptions(300)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = false
                }
            ));
            var casConfig = configuration.GetSection("AppMetaData:ModulesMDatas:Redis")
                .Get<CatastrophicHandlerConfig>();
            maxRetry = casConfig.MaxRetryCount;
            retryInterval = casConfig.RetryInterval;
            services.AddSingleton<IChannel<RedisInstance<MockUserKey, MockUserValue>, MockUserKey, MockUserValue>>(mockOutboxChannel);
            var provider = services.BuildServiceProvider();
            _conReaderManager = new RedisConnectionManager(mockLogger, options, configuration, moduleData);
            _conManager = new RedisConnectionManager(mockLogger, options, configuration, moduleData);
            _reader = new RedisReader<MockUserKey, MockUserValue>(_conReaderManager, new System.Text.Json.JsonSerializerOptions());
            _writerWithOutTageFlag = new RedisWriterOutBox<MockUserKey, MockUserValue>(
                    _conManager,
                    new JsonSerializerOptions(),
                    mockLogger,
                    configuration,
                    () => {
                        _escalationCall += 1;
                        return Task.CompletedTask;
                    },
                    provider,
                    _traceId
                    );

        }

        [Fact(DisplayName ="Happy path")]
        public async Task RedisReaderShouldBeAbleToReadExistEntry()
        {
            // arrange
            var mockKey = new MockUserKey() { UserKey = "Key" };
            var mockValue = new MockUserValue() { Value = "cacheValue" };
            var ct = new CancellationTokenSource();
            var mockTtl = 3;
            await this.InitializeAsync();

            try
            {
                await _writerWithOutTageFlag.EnqueueAsync(_traceId, mockKey, mockValue, TimeSpan.FromSeconds(mockTtl), ct.Token);
            }
            catch (Exception ex)
            {
                // alway fail during the set up phase
                Assert.True(false);
            }

            // act
            var readValue = _reader.ReadAsync(mockKey, ct.Token);

            // assert
            Assert.NotEqual(readValue, default);
            Assert.NotNull(readValue);
            Assert.Equal(readValue.Result.Value, mockValue.Value);
        }

        [Fact(DisplayName = "Soft crash path, where redis infra is healthy but the connection is stale.")]
        public async Task RedisReaderShouldBeAbleToReadAfterSelfHeal()
        {
            // arrange
            var mockKey = new MockUserKey() { UserKey = "Key" };
            var mockValue = new MockUserValue() { Value = "cacheValue" };
            var ct = new CancellationTokenSource();
            var mockTtl = 30000;
            await this.InitializeAsync();
            await _conManager.CloseAsync();
            await _writerWithOutTageFlag.EnqueueAsync(_traceId, mockKey, mockValue, TimeSpan.FromSeconds(mockTtl), ct.Token);

            await Task.Delay(maxRetry * retryInterval);


            // act
            var readValue = await _reader.ReadAsync(mockKey, ct.Token);

            // assert
            Assert.NotEqual(readValue, default);
            Assert.NotNull(readValue);
            Assert.Equal(readValue.Value, mockValue.Value);
            // wait till the self heal complete
        }

    }
}
