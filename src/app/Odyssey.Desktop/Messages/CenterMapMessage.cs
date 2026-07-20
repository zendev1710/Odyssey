using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Odyssey.Messages
{
    // Un message simple pour notifier qu'on souhaite recentrer la carte
    public record CenterMapMessage(bool IsGlobal);
}