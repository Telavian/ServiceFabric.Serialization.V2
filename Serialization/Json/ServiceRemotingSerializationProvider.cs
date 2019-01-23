using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using ServiceFabric.Serialization.V2.Serialization.Json.Request;
using ServiceFabric.Serialization.V2.Serialization.Json.Response;

namespace ServiceFabric.Serialization.V2.Serialization.Json
{
    public class ServiceRemotingSerializationProvider : IServiceRemotingMessageSerializationProvider
    {
        private readonly ILogger _logger;

        public ServiceRemotingSerializationProvider(ILogger logger)
        {
            _logger = logger;
        }

        public IServiceRemotingRequestMessageBodySerializer CreateRequestMessageSerializer(Type serviceInterfaceType, IEnumerable<Type> requestWrappedTypes, IEnumerable<Type> requestBodyTypes = null)
        {
            return new ServiceRemotingRequestJsonMessageBodySerializer(_logger, serviceInterfaceType, requestBodyTypes);
        }

        public IServiceRemotingResponseMessageBodySerializer CreateResponseMessageSerializer(Type serviceInterfaceType, IEnumerable<Type> responseWrappedTypes, IEnumerable<Type> responseBodyTypes = null)
        {
            return new ServiceRemotingResponseJsonMessageBodySerializer(_logger, serviceInterfaceType, responseBodyTypes);
        }

        public IServiceRemotingMessageBodyFactory CreateMessageBodyFactory()
        {
            return new MessageFactory();
        }
    }
}
