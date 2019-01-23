# ServiceFabric.Serialization.V2
This is a working service fabric service wrapper that allows for the use of the V2 custom serialization.
Currently it is using ServiceStack.Text to create JSON which is then GZipped compressed.

**Usage**

Program.cs
`````c#
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            var factory = new LoggerFactory()
                .AddSerilog(logger, true);

            ServiceRunner.RunService(factory, "XYZServiceType",
                (logFactory, context) =>
                {
                    return new XYZService(logFactory, context);
                });
        }
    }
`````

XYZService.cs
`````c#
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed partial class XYZService : StatelessServiceBase
    {
        #region Private Variables

        private XYZ _xyzService;

        #endregion Private Variables

        #region Constructors

        public XYZService(ILoggerFactory factory, StatelessServiceContext context)
            : base(factory, context)
        {
            // Nothing
        }

        #endregion Constructors

        #region Protected Methods

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceListeners()
        {
            var listeners = new[]
            {
                new ServiceInstanceListener((c) =>
                {
                    return new FabricTransportServiceRemotingListener(c, this, null,
                        new ServiceRemotingSerializationProvider(Logger));
                })
            };

            return listeners;
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunServiceAsync(CancellationToken cancellationToken)
        {
            var dependency1 = await ResolveServiceAsync<IDependency1>(new Uri("fabric:/..."), cancellationToken)
                .ConfigureAwait(false);
                
            _xyzService = await ResolveDependencyAsync(async () =>
                {
                    return await CreateServiceAsync(dependency1)
                      .ConfigureAwait(false);
                }, cancellationToken)
                .ConfigureAwait(false);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                //ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken)
                    .AnyContext();
            }
        }

        #endregion Protected Methods
    }
`````
