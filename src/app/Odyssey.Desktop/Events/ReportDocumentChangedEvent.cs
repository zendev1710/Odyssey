using Odyssey.Models.Documents;
using Prism.Events;

namespace Odyssey.Events
{
    /// <summary>
    /// Event that is published when the active document has changed.
    /// </summary>
    public class ReportDocumentChangedEvent : PubSubEvent<CRDocument>
    {        
    }
}
