using System.Diagnostics.Tracing;

namespace ServiceFabric.Serialization.V2.Trace
{
    // Event keywords can be used to categorize events.
    // Each keyword is a bit flag. A single event can be associated with multiple keywords (via EventAttribute.Keywords property).
    // Keywords must be defined as a public class named 'Keywords' inside EventSource that uses them.
    public static class Keywords
    {
        public const EventKeywords Requests = (EventKeywords)0x1L;
        public const EventKeywords ServiceInitialization = (EventKeywords)0x2L;
    }
}