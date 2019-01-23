using Microsoft.ServiceFabric.Services.Remoting.V2;
using ServiceFabric.Serialization.V2.Serialization.Json.Request;
using ServiceFabric.Serialization.V2.Serialization.Json.Response;

namespace ServiceFabric.Serialization.V2.Serialization.Json
{
    public class MessageFactory : IServiceRemotingMessageBodyFactory
    {
        public IServiceRemotingRequestMessageBody CreateRequest(string interfaceName, string methodName, int numberOfParameters, object wrappedRequestObject)
        {
            return new JsonRemotingRequestBody();
        }

        public IServiceRemotingResponseMessageBody CreateResponse(string interfaceName, string methodName, object wrappedResponseObject)
        {
            return new JsonRemotingResponseBody();
        }
    }
}