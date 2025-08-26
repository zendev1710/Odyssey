using Odyssey.ViewModels;
using Prism.Events;

namespace Odyssey.Events
{
    /// <summary>
    /// Event that is published when the selection state changed.
    /// </summary>
    public class SelectionChangeEvent : PubSubEvent<ISelectionChange>
    {
    }
}
