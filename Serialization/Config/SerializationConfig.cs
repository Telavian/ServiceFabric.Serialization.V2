using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.Text;

namespace ServiceFabric.Serialization.V2.Serialization.Config
{
    public class SerializationConfig<T>
    {
        public static void RegisterSerializer(Func<T, string> serializer)
        {
            JsConfig<T>.RawSerializeFn = serializer;
        }

        public static void RegisterDeserializer(Func<string, T> deserializer)
        {
            JsConfig<T>.RawDeserializeFn = deserializer;
        }
    }
}
