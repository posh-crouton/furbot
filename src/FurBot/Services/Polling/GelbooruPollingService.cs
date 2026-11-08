using CommunityToolkit.Mvvm.Messaging;
using FurBot.Clients;
using FurBot.Configuration;
using FurBot.Types.Gelbooru;
using FurBot.Types.Polling;
using Microsoft.Extensions.Options;

namespace FurBot.Services.Polling;

internal sealed class GelbooruPollingService : PollingServiceBase
{
    private readonly IOptions<GelbooruOptions> _gelbooruOptions;
    private readonly IGelbooruClient _gelbooruClient;

    public GelbooruPollingService(IMessenger messenger,
        IOptions<PollingServiceOptions> options,
        IOptions<GelbooruOptions> gelbooruOptions,
        IGelbooruClient gelbooruClient)
        : base(messenger, options)
    {
        _gelbooruOptions = gelbooruOptions;
        _gelbooruClient = gelbooruClient;
    }

    protected override async Task<IEnumerable<Post>> PollAsync(CancellationToken ct = default)
    {
        _gelbooruClient.Init();

        IEnumerable<GelbooruPost> posts = await _gelbooruClient.GetPostsAsync(_gelbooruOptions.Value.Query,
            _gelbooruOptions.Value.PostLimit,
            ct);

        return posts.Select(post => new Post
        {
            Title = $"By {string.Join(", ", post.Tags.Artist)}",
            ImageUri = post.File.Url ?? string.Empty,
            SourceWebsiteName = _gelbooruOptions.Value.InstanceName,
            PostUri = $"{_gelbooruOptions.Value.InstanceUri}/posts/{post.Id}"
        });
    }
}
