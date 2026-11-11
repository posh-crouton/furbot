using CommunityToolkit.Mvvm.Messaging;
using FurBot.Clients;
using FurBot.Configuration;
using FurBot.Database;
using FurBot.Database.Entities;
using FurBot.Types.Gelbooru;
using FurBot.Types.Polling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FurBot.Services.Polling;

internal sealed class GelbooruPollingService : PollingServiceBase
{
    private readonly IOptionsMonitor<GelbooruOptions> _gelbooruOptions;
    private readonly IGelbooruClient _gelbooruClient;
    private readonly IFurbotContext _dbContext;

    public GelbooruPollingService(IMessenger messenger,
        IOptionsMonitor<PollingServiceOptions> options,
        IOptionsMonitor<GelbooruOptions> gelbooruOptions,
        IGelbooruClient gelbooruClient,
        IFurbotContext dbContext)
        : base(messenger, options)
    {
        _gelbooruOptions = gelbooruOptions;
        _gelbooruClient = gelbooruClient;
        _dbContext = dbContext;
    }

    protected override async Task<IEnumerable<Post>> PollAsync(CancellationToken ct = default)
    {
        _gelbooruClient.Init();

        IEnumerable<GelbooruPost> posts = await _gelbooruClient.GetPostsAsync(_gelbooruOptions.CurrentValue.Query,
            _gelbooruOptions.CurrentValue.PostLimit,
            ct);

        var results = new List<Post>();
        foreach (GelbooruPost post in posts)
        {
            if (await DedupeAsync(post, ct) is null)
            {
                continue;
            }

            results.Add(new Post
            {
                Title = $"By {string.Join(", ", post.Tags.Artist)}",
                ImageUri = post.File.Url ?? string.Empty,
                SourceWebsiteName = _gelbooruOptions.CurrentValue.InstanceName,
                PostUri = $"{_gelbooruOptions.CurrentValue.InstanceUri}/posts/{post.Id}"
            });
        }

        return results;
    }

    private async Task<GelbooruPost?> DedupeAsync(GelbooruPost post, CancellationToken ct = default)
    {
        if (await _dbContext.Posts.AnyAsync(x => x.Id == post.Id
                && x.InstanceUri == _gelbooruOptions.CurrentValue.InstanceUri
                && x.Md5 == post.Md5, ct))
        {
            return null;
        }

        await _dbContext.Posts.AddAsync(new PostEntity
        {
            InstanceUri = _gelbooruOptions.CurrentValue.InstanceUri,
            Id = post.Id,
            Md5 = post.Md5
        }, ct);

        _ = await _dbContext.SaveChangesAsync(ct);
        return post;
    }
}
