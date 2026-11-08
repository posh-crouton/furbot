using CommunityToolkit.Mvvm.Messaging;
using FurBot.Configuration;
using FurBot.Messages;
using FurBot.Types.Polling;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FurBot.Services.Polling;

internal abstract class PollingServiceBase : BackgroundService
{
    private readonly IMessenger _messenger;
    private readonly IOptions<PollingServiceOptions> _options;

    protected PollingServiceBase(IMessenger messenger, IOptions<PollingServiceOptions> options)
    {
        _messenger = messenger;
        _options = options;
    }

    protected abstract Task<IEnumerable<Post>> PollAsync(CancellationToken ct = default);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            IEnumerable<Post> posts = await PollAsync(stoppingToken);
            foreach (Post post in posts)
            {
                _messenger.Send(new PostPublishedMessage { Post = post });
            }

            await Task.Delay(_options.Value.IntervalMs, stoppingToken);
        }
    }
}
