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
using Serilog.Events;
using Shared.Infrastructure.Observability;
using Shared.Infrastructure.Redis.Interface.Channel;
using Shared.Infrastructure.Redis.Model;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Redis.Core.Extension.Channel;

namespace IdeaSpace.Share.Infrastructure.Redis.Intergration
{
    
   
    public class RedisWriterIntergrationTests : IAsyncLifetime
    {
        private readonly RedisContainer _container;
        private Serilog.Core.Logger mockLogger;
        private IRedisConnectionManger _conManager;
        private IRedisWriteOutBox<MockUserKey, MockUserValue> _writer;

        private IRedisWriteOutBox<MockUserKey, MockUserValue> _writerWithOutTageFlag;
        private int _escalationCall = 0;
        private int maxRetry ;
        private bool isOutTage = false;
        private TraceId _traceId =  new TraceId(Guid.NewGuid(), IssuerType.Internal.GetHashCode());
        public RedisWriterIntergrationTests()
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
            if(_container != null) await _container.StopAsync();
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
            maxRetry = configuration
                .GetSection("AppMetaData:ModulesMDatas:Redis:MaxRetryCount")
                .Get<int>();
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
            services.AddSingleton<IChannel<RedisInstance<MockUserKey, MockUserValue>, MockUserKey, MockUserValue>>(mockOutboxChannel);
            var provider = services.BuildServiceProvider();
            _conManager = new RedisConnectionManager(mockLogger, options, configuration, moduleData);
            _writer = new RedisWriterOutBox<MockUserKey, MockUserValue>(
                _conManager,
                new JsonSerializerOptions(),
                mockLogger,
                configuration,
                () => { 
                    _escalationCall+=1; 
                    return Task.CompletedTask; },
                provider,
                _traceId
                );
            _writerWithOutTageFlag = new RedisWriterOutBox<MockUserKey, MockUserValue>(
                _conManager,
                new JsonSerializerOptions(),
                mockLogger,
                configuration,
                () => {
                    isOutTage = true;
                    _escalationCall += 1;
                    return Task.CompletedTask;
                },
                provider,
                _traceId
                );
        }


        [Fact(DisplayName ="Standard where nothing go wrong, we should able to read value we write to Redis.")]
        public async Task RedisWriteShouldBeReadable()
        {
            // Arrange
            var userKey = new MockUserKey{ UserKey = "key"};
            var searchKey = new MockUserKey{UserKey = "key"};
            var userValue = new MockUserValue{Value = "message"};
            var mockTtl = 1000;
            var openConnection = _conManager.GetDatabase(0); // probably same db
            var ct = new CancellationTokenSource();
            
            // Act
            using (TestCorrelator.CreateContext())
            {

                await _writer.EnqueueAsync(_traceId, userKey, userValue, TimeSpan.FromSeconds(mockTtl), ct.Token);
                var read = await openConnection.StringGetAsync(searchKey.ToRedisKey());
                var events = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
                // Assert
                Assert.True(events.Count == 0);
                Assert.True(read.HasValue);
                var value = JsonSerializer.Deserialize<MockUserValue>(read);
                Assert.Equal("message", value.Value);
            }

        }

        [Fact(DisplayName = "Redis container alive, but connection is disable, Writer should able to restart the connection.")]
        public async Task RedisWriteShouldSelfHealWithStaleConnection()
        {
            // Arrange
            await this.DisposeAsync();
            await this.InitializeAsync();
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
                await _writer.EnqueueAsync(_traceId, userKey, userValue, ttl, ct.Token);
                // since self healing is an async task, we must give it enough time
                await Task.Delay(maxRetry * 140);

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
        [Fact(DisplayName = "Redis should heal (even when not reach outage status internally) after hard reset and flush outbox")]
        public async Task RedisWriteShouldBackUpAfterHardResetRedis()
        {
            // Arrange
            await this.DisposeAsync();
            await this.InitializeAsync();
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
            var ct = new CancellationTokenSource();
            var newConnection = _container.GetConnectionString();
            await _container.StopAsync(); // ensure redis container close
            var ttl = TimeSpan.FromSeconds(mockTtl);
            // Act
            using (TestCorrelator.CreateContext())
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(5000); // wait for thread handle outage conclude catastrophic
                    await _container.StartAsync();
                    var newConnection = _container.GetConnectionString();
                    _writer.HotSwapConnection(newConnection); // simulate worker call Hotswap
                });
                await _writer.EnqueueAsync(_traceId, userKey, userValue, ttl, ct.Token);
                await Task.Delay(maxRetry * 120);

                var events = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
                var lastMessageObj = events.LastOrDefault();
                long? outBoxCount = null;
                if (lastMessageObj.Properties.TryGetValue("outBoxCount", out var outBoxCountRaw)
                    && outBoxCountRaw is ScalarValue scalar
                    && scalar.Value is long count)
                {
                    outBoxCount = count;
                }
                try
                {
                    var newPath = _container.GetConnectionString();
                    _conManager.HotSwapConnection(newPath);
                    _conManager.AttemptHeal();
                    var openConnection = _conManager.GetDatabase(-1); // probably same db
                    var read = await openConnection.StringGetAsync(searchKey.ToRedisKey());
                    // Assert
                    Assert.True(events.Count > 0);
                    Assert.True(read.HasValue);
                    var value = JsonSerializer.Deserialize<MockUserValue>(read);
                    Assert.Equal("message", value.Value);
                }
                catch (Exception ex)
                {
                    //
                    var test = ex;
                    Assert.True(false); // always fail on exception
                }
                
            }
        }

        [Fact(DisplayName ="Multi thread that try to write (same key) during an outage should not create duplicated write.")]
        public async Task MultithreadDataShouldNotLostDuringOutageWithDuplicatedKey()
        {
            // Arrange
            await this.DisposeAsync();
            await this.InitializeAsync();
            isOutTage = false;
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
                var backgroundTask = Task.Run(async () =>
                {
                    await Task.Delay(maxRetry * 50);
                    await _writerWithOutTageFlag.EnqueueAsync(_traceId, userKey, userValue, ttl, ct.Token);
                });

                await _writerWithOutTageFlag.EnqueueAsync(_traceId, userKey, userValue, ttl, ct.Token);

                var counter = 0;
                while (!isOutTage || counter > 600)
                {

                    counter++;
                    await Task.Delay(200);
                }
                var events = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
                var lastMessageObj = events.LastOrDefault();
                var result = _writerWithOutTageFlag.IsConnectionHealthy();
                long? outBoxCount = null;
                if (lastMessageObj.Properties.TryGetValue("outBoxCount", out var outBoxCountRaw)
                    && outBoxCountRaw is ScalarValue scalar
                    && scalar.Value is long count)
                {
                    outBoxCount = count;
                }
                // Assert
                Assert.NotNull(outBoxCount);
                Assert.Equal(1, outBoxCount);
                Assert.True(events.Count > 0);
                // the singleton (redisWriter) should only have 1 RetryAsync loop running at any time per process
                // so expect to only 1 final escalation cb trigger.
                Assert.Equal(1, _escalationCall); 
            }
        }

        [Fact(DisplayName = "Multi thread, during outage, cached value should be present in outbox(2).")]
        public async Task MultithreadDataShouldNotLostDuringOutageWithNoDuplicatedKey()
        {

            // Arrange
            await this.DisposeAsync();
            await this.InitializeAsync();
            isOutTage = false;
            var userKey = new MockUserKey
            {
                UserKey = "key"
            };
            var userKey2 = new MockUserKey
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
                var backgroundTask = Task.Run(async () =>
                {
                    await Task.Delay(maxRetry * 50);
                    await _writerWithOutTageFlag.EnqueueAsync(_traceId,userKey2, userValue, ttl, ct.Token);
                });

                await _writerWithOutTageFlag.EnqueueAsync(_traceId, userKey, userValue, ttl, ct.Token);

                // manually wait 
                var counter = 0;
                while(!isOutTage || counter > 600)
                {

                    counter++;
                    await Task.Delay(200);
                }
                var events = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
                var lastMessageObj = events.LastOrDefault();
                long? outBoxCount = null;
                if (lastMessageObj.Properties.TryGetValue("outBoxCount", out var outBoxCountRaw)
                    && outBoxCountRaw is ScalarValue scalar
                    && scalar.Value is long count)
                {
                    outBoxCount = count;
                }
                // Assert
                Assert.NotNull(outBoxCount);
                Assert.Equal(2, outBoxCount);
                Assert.True(events.Count > 0);
                // the singleton (redisWriter) should only have 1 RetryAsync loop running at any time per process
                // so expect to only 1 final escalation cb trigger.
                Assert.Equal(1, _escalationCall);
            }
        }

        [Fact(DisplayName ="Writer should arrive at outage status and trigger escalation call back after repeated fail attempt with self heal.")]
        public async Task WriteShouldFailIfContainerDrop()
        {
            // Arrange
            await this.DisposeAsync();
            await this.InitializeAsync();
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
                await _writerWithOutTageFlag.EnqueueAsync(_traceId, userKey, userValue, ttl, ct.Token);
                // manually wait 
                var counter = 0;
                while (!isOutTage || counter > 600)
                {

                    counter++;
                    await Task.Delay(200);
                }
                var events = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
                var lastMessageObj = events.LastOrDefault();
                long? outBoxCount = null;
                if (lastMessageObj.Properties.TryGetValue("outBoxCount", out var outBoxCountRaw)
                    && outBoxCountRaw is ScalarValue scalar
                    && scalar.Value is long count)
                {
                    outBoxCount = count;
                }

                // Assert
                var test = outBoxCount.Equals(1);
                Assert.True(events.Count > 0);
                Assert.True(outBoxCount == (1));
                Assert.Equal(_escalationCall, 1);
            }
        }
    }
}
