using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.Serialization.V2.Services.Stateless;
using ServiceFabric.Serialization.V2.Trace;

namespace ServiceFabric.Serialization.V2.Services
{
    public static class ServiceRunner
    {
        public static void RunService<T>(ILoggerFactory factory, string serviceType, Func<ILoggerFactory, StatelessServiceContext, T> creator)
            where T : StatelessServiceBase
        {
            var logger = factory.CreateLogger(typeof(ServiceRunner));

            try
            {
                logger.LogDebug($"Registering service {typeof(T).Name}");
                
                ServiceRuntime.RegisterServiceAsync(serviceType,
                        context =>
                        {
                            return creator(factory, context);
                        })
                    .GetAwaiter()
                    .GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(T).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                logger.LogError(0, ex, $"Unable to register service {typeof(T).Name}");
                ServiceEventSource.Current.ServiceHostInitializationFailed(ex.ToString());
                throw;
            }
        }
    }
}