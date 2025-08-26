using Odyssey.Models.Documents;
using Prism.Events;
using static Odyssey.Models.Documents.CRDocument;

namespace Odyssey.Events
{
    /// <summary>
    /// Event that is published when the selection state changed.
    /// </summary>
    public class MessageEvent : PubSubEvent<EventMessage>
    {
    }
}
