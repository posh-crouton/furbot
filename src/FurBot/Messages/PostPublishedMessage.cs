using FurBot.Types.Polling;

namespace FurBot.Messages;

internal sealed class PostPublishedMessage
{
    public required Post Post { get; init; }
}
