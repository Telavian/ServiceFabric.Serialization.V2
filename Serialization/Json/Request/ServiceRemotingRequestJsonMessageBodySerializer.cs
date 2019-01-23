using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Messaging;
using ServiceFabric.Serialization.V2.Extensions;

namespace ServiceFabric.Serialization.V2.Serialization.Json.Request
{
    public class ServiceRemotingRequestJsonMessageBodySerializer : IServiceRemotingRequestMessageBodySerializer
    {
        private readonly ILogger _logger;

        public ServiceRemotingRequestJsonMessageBodySerializer(ILogger logger, Type serviceInterfaceType, IEnumerable<Type> parameterInfo)
        {
            _logger = logger;            
        }

        public IOutgoingMessageBody Serialize(IServiceRemotingRequestMessageBody serviceRemotingRequestMessageBody)
        {
            if (serviceRemotingRequestMessageBody == null)
            {
                return null;
            }

            var bytes = serviceRemotingRequestMessageBody
                .Serialize();

            var segment = new ArraySegment<byte>(bytes);
            return new OutgoingMessageBody(new [] { segment });
        }

        public IServiceRemotingRequestMessageBody Deserialize(IIncomingMessageBody messageBody)
        {
            if (messageBody == null)
            {
                return new JsonRemotingRequestBody();
            }

            using (var buffer = messageBody.GetReceivedBuffer())
            {
                var bytes = buffer.ReadStreamToByteArray();
                return bytes.Deserialize<JsonRemotingRequestBody>() ?? new JsonRemotingRequestBody();
            }
        }
    }
}