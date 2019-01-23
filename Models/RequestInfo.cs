namespace ServiceFabric.Serialization.V2.Models
{
    public class RequestInfo
    {
        public byte[] Request { get; protected set; }
        public object Item { get; protected set; }

        public RequestInfo(byte[] request, object item)
        {
            Request = request;
            Item = item;
        }
    }
}