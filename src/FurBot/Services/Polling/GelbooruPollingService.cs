using CommunityToolkit.Mvvm.Messaging;
using FurBot.Clients;
using FurBot.Configuration;
using FurBot.Types.Gelbooru;
using FurBot.Types.Polling;
using Microsoft.Extensions.Options;

namespace FurBot.Services.Polling;

internal sealed class GelbooruPollingService : PollingServiceBase
{
    private readonly IOptionsMonitor<GelbooruOptions> _gelbooruOptions;
    private readonly IGelbooruClient _gelbooruClient;

    public GelbooruPollingService(IMessenger messenger,
        IOptionsMonitor<PollingServiceOptions> options,
        IOptionsMonitor<GelbooruOptions> gelbooruOptions,
        IGelbooruClient gelbooruClient)
        : base(messenger, options)
    {
        _gelbooruOptions = gelbooruOptions;
        _gelbooruClient = gelbooruClient;
    }

    protected override async Task<IEnumerable<Post>> PollAsync(CancellationToken ct = default)
    {
        _gelbooruClient.Init();

        IEnumerable<GelbooruPost> posts = await _gelbooruClient.GetPostsAsync(_gelbooruOptions.CurrentValue.Query,
            _gelbooruOptions.CurrentValue.PostLimit,
            ct);

        return posts.Select(post => new Post
        {
            Title = $"By {string.Join(", ", post.Tags.Artist)}",
            ImageUri = post.File.Url ?? string.Empty,
            SourceWebsiteName = _gelbooruOptions.CurrentValue.InstanceName,
            PostUri = $"{_gelbooruOptions.CurrentValue.InstanceUri}/posts/{post.Id}"
        });
    }
}
