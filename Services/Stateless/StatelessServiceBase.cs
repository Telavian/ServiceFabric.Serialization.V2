using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using ServiceFabric.Serialization.V2.Interfaces;
using ServiceFabric.Serialization.V2.Serialization.Json;
using ServiceFabric.Serialization.V2.Trace;
using StatelessService = Microsoft.ServiceFabric.Services.Runtime.StatelessService;

namespace ServiceFabric.Serialization.V2.Services.Stateless
{
    public abstract class StatelessServiceBase : StatelessService, IService
    {
        #region Private Variables

        private readonly ServiceProxyFactory _proxyFactory;

        #endregion Private Variables

        #region Public Properties

        public ILoggerFactory LogFactory { get; protected set; }
        public ILogger Logger { get; protected set; }

        #endregion Public Properties

        public StatelessServiceBase(ILoggerFactory logFactory, StatelessServiceContext serviceContext)
            : base(serviceContext)
        {
            var version = Assembly.GetEntryAssembly().GetName().Version;

            LogFactory = logFactory;
            Logger = logFactory.CreateLogger(typeof(StatelessServiceBase));
            Logger.LogInformation("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            Logger.LogInformation("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            Logger.LogInformation($"Initializing {GetType().Name} - {version}");
            Logger.LogInformation("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            Logger.LogInformation("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");

            _proxyFactory = new ServiceProxyFactory(c =>
            {
                return new FabricTransportServiceRemotingClientFactory(                       
                    serializationProvider: new ServiceRemotingSerializationProvider(Logger)
                );
            });

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Logger.LogError(0, args.ExceptionObject as Exception, "Unhandled error");
            };
        }

        #region Public Methods

        protected abstract Task RunServiceAsync(CancellationToken cancellationToken);

        protected abstract IEnumerable<ServiceInstanceListener> CreateServiceListeners();

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Initializing {GetType().Name} on partition {GetPartitionId()}");

            try
            {
                await RunServiceAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogError(0, ex, "Unable to run service");

                // Wait for logging to catch up
                await Task.Delay(TimeSpan.FromSeconds(5), CancellationToken.None)
                    .ConfigureAwait(false);

                throw;
            }
        }        

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            try
            {
                var listeners = CreateServiceListeners()
                    .ToArray();

                return listeners;
            }
            catch (Exception ex)
            {
                Logger.LogError(0, ex, "Unable to create service listeners");

                // Wait for logging to catch up
                Thread.Sleep(TimeSpan.FromSeconds(5));

                throw;
            }
        }

        public async Task<TService> ResolveServiceAsync<TService>(Uri location, CancellationToken cancellation)
            where TService : ITestableService
        {
            return await ResolveServiceAsync<TService>(location, ServicePartitionKey.Singleton, TargetReplicaSelector.Default, null, TimeSpan.MaxValue, cancellation)
                .ConfigureAwait(false);
        }

        public async Task<TService> ResolveServiceAsync<TService>(Uri location, TimeSpan timeout, CancellationToken cancellation)
            where TService : ITestableService
        {
            return await ResolveServiceAsync<TService>(location, ServicePartitionKey.Singleton, TargetReplicaSelector.Default, null, timeout, cancellation)
                .ConfigureAwait(false);
        }

        public async Task<TService> ResolveServiceAsync<TService>(Uri location, ServicePartitionKey partition, CancellationToken cancellation)
            where TService : ITestableService
        {
            return await ResolveServiceAsync<TService>(location, partition, TargetReplicaSelector.Default, null, TimeSpan.MaxValue, cancellation)
                .ConfigureAwait(false);
        }

        public async Task<TService> ResolveServiceAsync<TService>(Uri location, ServicePartitionKey partition, TimeSpan timeout, CancellationToken cancellation)
            where TService : ITestableService
        {
            return await ResolveServiceAsync<TService>(location, partition, TargetReplicaSelector.Default, null, timeout, cancellation)
                .ConfigureAwait(false);
        }

        public async Task<TService> ResolveServiceAsync<TService>(Uri location, ServicePartitionKey partition, TargetReplicaSelector selector, CancellationToken cancellation)
            where TService : ITestableService
        {
            return await ResolveServiceAsync<TService>(location, partition, selector, null, TimeSpan.MaxValue, cancellation)
                .ConfigureAwait(false);
        }

        public async Task<TService> ResolveServiceAsync<TService>(Uri location, ServicePartitionKey partition, TargetReplicaSelector selector, TimeSpan timeout, CancellationToken cancellation)
            where TService : ITestableService
        {
            return await ResolveServiceAsync<TService>(location, partition, selector, null, timeout, cancellation)
                .ConfigureAwait(false);
        }

        public async Task<TService> ResolveServiceAsync<TService>(Uri location, ServicePartitionKey partition, TargetReplicaSelector selector, string listener, CancellationToken cancellation)
            where TService : ITestableService
        {
            return await ResolveServiceAsync<TService>(location, partition, selector, listener, TimeSpan.MaxValue, cancellation)
                .ConfigureAwait(false);
        }

        public async Task<TService> ResolveServiceAsync<TService>(Uri location, ServicePartitionKey partition, TargetReplicaSelector selector, string listener, TimeSpan timeout, CancellationToken cancellation)
            where TService : ITestableService
        {
            return await ResolveDependencyAsync(async () =>
                {

                    if (_proxyFactory == null)
                    {
                        throw new Exception("Proxy factory is not initialized");
                    }

                    Logger.LogInformation($"{typeof(TService).Name}: Creating proxy for service - {location}");
                    var proxy = _proxyFactory.CreateServiceProxy<TService>(location, partition, selector, listener);
                    Logger.LogInformation($"{typeof(TService).Name}: Proxy for service created");

                    if (proxy == null)
                    {
                        throw new Exception($"Proxy '{typeof(TService).Name}' was not resolved correctly");
                    }

                    var result = await proxy.TestAsync()
                        .ConfigureAwait(false);

                    if (result == false)
                    {
                        throw new Exception("Service was not initialized correctly");
                    }

                    Logger.LogInformation($"{typeof(TService).Name}: Proxy for service connected");
                    return proxy;
                }, timeout, cancellation)
                .ConfigureAwait(false);
        }

        public async Task<Uri[]> ResolveSingletonServiceEndpointsAsync(Uri location, CancellationToken cancellation)
        {
            return await ResolveServiceEndpointsAsync(location, ServicePartitionKey.Singleton, false, cancellation)
                .ConfigureAwait(false);
        }

        public async Task<Uri[]> ResolveSingletonServiceEndpointsAsync(Uri location, bool require, CancellationToken cancellation)
        {
            return await ResolveServiceEndpointsAsync(location, ServicePartitionKey.Singleton, require, cancellation)
                .ConfigureAwait(false);
        }

        public async Task<Uri[]> ResolveServiceEndpointsAsync(Uri location, ServicePartitionKey key, CancellationToken cancellation)
        {
            return await ResolveServiceEndpointsAsync(location, key, false, cancellation)
                .ConfigureAwait(false);
        }

        public async Task<Uri[]> ResolveServiceEndpointsAsync(Uri location, ServicePartitionKey key, bool require, CancellationToken cancellation)
        {
            return await ResolveDependencyAsync(async () =>
                {
                    var resolver = ServicePartitionResolver.GetDefault();
                    var partitions = await resolver.ResolveAsync(location, key, cancellation)
                        .ConfigureAwait(false);

                    var addresses = partitions?.Endpoints
                        ?.Select(x => x.Address)
                        ?.ToArray();

                    var endpoints = ResolveEndpoints(addresses);

                    if (require && endpoints?.Any() != true)
                    {
                        throw new Exception($"No endpoints resolved for '{location}'");
                    }

                    return endpoints;
                }, cancellation)
                .ConfigureAwait(false);
        }

        public async Task<T> ResolveDependencyAsync<T>(Func<Task<T>> creator, CancellationToken cancellation)
        {
            return await ResolveDependencyAsync(creator, TimeSpan.MaxValue, cancellation)
                .ConfigureAwait(false);
        }

        public async Task<TDependency> ResolveDependencyAsync<TDependency>(Func<Task<TDependency>> creator, TimeSpan timeout, CancellationToken cancellation)
        {
            var startTime = DateTime.Now;
            var totalAttempts = 0;
            var serviceStateProperty = "Service State - " + typeof(TDependency).Name;
            var message = "";
            Exception lastException = null;

            while (DateTime.Now.Subtract(startTime) < timeout)
            {
                cancellation.ThrowIfCancellationRequested();

                try
                {
                    var dependency = await creator()
                        .ConfigureAwait(false);

                    message = $"Dependency '{typeof(TDependency).Name}' created";

                    LogInfo(message);
                    ReportHealth(serviceStateProperty, HealthState.Ok, message);

                    return dependency;
                }
                catch (Exception ex)
                {
                    LogInfo($"Dependency '{typeof(TDependency).Name}' creation failed: {ex.Message}");
                    lastException = ex;
                }

                totalAttempts++;
                var delay = CalculateResolveDelay(totalAttempts);

                await Task.Delay(delay, cancellation)
                    .ConfigureAwait(false);

                var timeWaited = DateTime.Now.Subtract(startTime);
                message = $"Waiting to create dependency {typeof(TDependency).Name}\r\nTime waited: {timeWaited}\r\n{lastException}";

                LogWarning(message);
                ReportHealth(serviceStateProperty, HealthState.Warning, message);
            }

            var totalTimeWaited = DateTime.Now.Subtract(startTime);
            message = $"Unable to resolve service {typeof(TDependency).Name} after {totalTimeWaited}";

            LogError(message);
            ReportHealth(serviceStateProperty, HealthState.Error, message);

            throw new Exception(message, lastException);
        }

        public void ReportHealth(string property, HealthState state, string message)
        {
            ReportHealth(property, state, TimeSpan.FromMinutes(15), message);
        }

        public void ReportHealth(string property, HealthState state, TimeSpan ttl, string message)
        {
            var record = new HealthInformation("ServiceBase", property, state);
            record.TimeToLive = ttl;
            record.Description = message;
            record.RemoveWhenExpired = true;

            Partition.ReportInstanceHealth(record);
        }

        public void LogCritical(string message)
        {
            ServiceEventSource.Current.ServiceMessage(Context, message);
            Logger.LogCritical(message);
        }

        public void LogDebug(string message)
        {
            ServiceEventSource.Current.ServiceMessage(Context, message);
            Logger.LogDebug(message);
        }

        public void LogError(string message)
        {
            ServiceEventSource.Current.ServiceMessage(Context, message);
            Logger.LogError(message);
        }

        public void LogInfo(string message)
        {
            ServiceEventSource.Current.ServiceMessage(Context, message);
            Logger.LogInformation(message);
        }

        public void LogTrace(string message)
        {
            ServiceEventSource.Current.ServiceMessage(Context, message);
            Logger.LogTrace(message);
        }

        public void LogWarning(string message)
        {
            ServiceEventSource.Current.ServiceMessage(Context, message);
            Logger.LogWarning(message);
        }

        public async Task<bool> WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
        {
            var start = DateTime.Now;

            while (DateTime.Now.Subtract(start) < timeout)
            {
                if (condition())
                {
                    return true;
                }

                await Task.Delay(100)
                    .ConfigureAwait(false);
            }

            return false;
        }

        #endregion Public Methods

        #region Private Methods

        private Uri[] ResolveEndpoints(string[] locations)
        {
            var results = new List<Uri>();

            //foreach (var location in locations)
            //{
            //    var json =  JsonConvert.DeserializeObject<JObject>(location);

            //    foreach (var value in json)
            //    {
            //        foreach (var address in value.Value)
            //        {
            //            var text = address.ToObject<string>();
            //            if (text.StartsWith("ws:"))
            //            {
            //                results.Add(new Uri(text));
            //            }
            //        }
            //    }
            //}

            return results.ToArray();
        }

        private TimeSpan CalculateResolveDelay(int totalAttempts)
        {
            totalAttempts = Math.Min(totalAttempts, 6) - 1;
            var delay = Math.Pow(2, totalAttempts);

            return TimeSpan.FromSeconds(delay);
        }

        private Guid GetPartitionId()
        {
            if (Partition.PartitionInfo.Kind == ServicePartitionKind.Singleton)
            {
                return ((SingletonPartitionInformation)Partition.PartitionInfo).Id;
            }

            return Partition.PartitionInfo.Id;
        }

        #endregion Private Methods
    }
}