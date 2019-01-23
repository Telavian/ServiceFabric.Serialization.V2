using System;
using System.Collections.Generic;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using ServiceFabric.Serialization.V2.Serialization.Config;
using ServiceStack;

namespace ServiceFabric.Serialization.V2.Serialization.Json.Response
{
    public class JsonRemotingResponseBody : IServiceRemotingResponseMessageBody
    {
        public object Value { get; set; }

        #region Constructors

        static JsonRemotingResponseBody()
        {
            SerializationConfig<JsonRemotingResponseBody>.RegisterSerializer(arg =>
            {
                var dict = new Dictionary<string, string>
                {
                    {nameof(Value), arg.Value.SerializeObject()}
                };

                return dict.SerializeToString();
            });

            SerializationConfig<JsonRemotingResponseBody>.RegisterDeserializer(arg =>
            {
                var dict = arg.Deserialize<Dictionary<string, string>>();
                dict.TryGetValue(nameof(Value), out var value);

                return new JsonRemotingResponseBody
                {
                    Value = value.DeserializeObject()
                };
            });
        }

        #endregion

        public void Set(object response)
        {
            Value = response;
        }

        public object Get(Type paramType)
        {
            return Value;
        }
    }
}