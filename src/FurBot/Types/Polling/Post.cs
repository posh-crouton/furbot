namespace FurBot.Types.Polling;

internal sealed class Post
{
    public required string Title { get; init; }
    public required string ImageUri { get; init; }
    public required string SourceWebsiteName { get; init; }
    public required string PostUri { get; init; }
}
