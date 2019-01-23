using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using ServiceFabric.Serialization.V2.Serialization.Config;
using ServiceStack;

namespace ServiceFabric.Serialization.V2.Serialization.Json.Request
{
    public class JsonRemotingRequestBody : IServiceRemotingRequestMessageBody
    {
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        #region Constructors

        static JsonRemotingRequestBody()
        {
            SerializationConfig<JsonRemotingRequestBody>.RegisterSerializer(arg =>
            {
                if (arg == null)
                {
                    return new Dictionary<string, object>()
                        .SerializeToString();
                }

                return arg.Parameters
                    .Select(x => new[]
                    {
                        x.Key,
                        x.Value.SerializeObject()
                    })
                    .SerializeToString();
            });

            SerializationConfig<JsonRemotingRequestBody>.RegisterDeserializer(arg =>
            {
                var items = arg.Deserialize<string[][]>();
                var results = items
                    .Select(x =>
                    {
                        return new KeyValuePair<string, object>(x[0], x[1].DeserializeObject());
                    })
                    .ToDictionary(x => x.Key, x => x.Value);
                
                return new JsonRemotingRequestBody
                {
                    Parameters = results
                };
            });
        }

        #endregion

        public void SetParameter(int position, string paramName, object parameter)
        {
            var name = BuildParameterName(position, paramName);
            Parameters[name] = parameter;
        }

        public object GetParameter(int position, string paramName, Type paramType)
        {
            var name = BuildParameterName(position, paramName);
            return Parameters[name];
        }

        #region Private Methods

        private string BuildParameterName(int position, string name)
        {
            return $"{position}_{name}";
        }

        #endregion
    }
}