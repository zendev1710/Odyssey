using Prism.Events;

namespace Odyssey.Events
{
    // Publie l'id d'une unité (int) quand la sélection change.
    public class UnitSelectedEvent : PubSubEvent<int>
    {
    }
}