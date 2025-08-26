using Odyssey.Models.Documents;
using Prism.Events;

namespace Odyssey.Events
{
    /// <summary>
    /// Event that is published when the active document is closed.
    /// </summary>
    public class ActiveDocumentClosedEvent : PubSubEvent<CRDocument>
    {        
    }
}
