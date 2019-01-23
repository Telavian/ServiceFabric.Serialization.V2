using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Messaging;
using ServiceFabric.Serialization.V2.Extensions;

namespace ServiceFabric.Serialization.V2.Serialization.Json.Response
{
    public class ServiceRemotingResponseJsonMessageBodySerializer : IServiceRemotingResponseMessageBodySerializer
    {
        private readonly ILogger _logger;

        public ServiceRemotingResponseJsonMessageBodySerializer(ILogger logger, Type serviceInterfaceType, IEnumerable<Type> parameterInfo)
        {
            _logger = logger;
        }

        public IOutgoingMessageBody Serialize(IServiceRemotingResponseMessageBody responseMessageBody)
        {
            if (responseMessageBody == null)
            {
                return null;
            }

            var bytes = responseMessageBody.Serialize();
            var segment = new ArraySegment<byte>(bytes);
            return new OutgoingMessageBody(new [] { segment });
        }

        public IServiceRemotingResponseMessageBody Deserialize(IIncomingMessageBody messageBody)
        {
            if (messageBody == null)
            {
                return new JsonRemotingResponseBody();
            }

            using (var buffer = messageBody.GetReceivedBuffer())
            {
                var bytes = buffer.ReadStreamToByteArray();
                return bytes.Deserialize<JsonRemotingResponseBody>() ?? new JsonRemotingResponseBody();
            }
        }
    }
}