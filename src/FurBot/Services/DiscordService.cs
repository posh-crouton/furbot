using CommunityToolkit.Mvvm.Messaging;
using Discord;
using Discord.WebSocket;
using FurBot.Configuration;
using FurBot.Messages;
using FurBot.Types.Polling;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FurBot.Services;

internal sealed class DiscordService : IDiscordService
{
    private readonly IOptions<DiscordOptions> _options;
    private readonly DiscordSocketClient _client;
    private readonly ILogger<DiscordService> _logger;
    private readonly IMessenger _messenger;

    public DiscordService(IOptions<DiscordOptions> options,
        DiscordSocketClient client,
        ILogger<DiscordService> logger,
        IMessenger messenger)
    {
        _options = options;
        _client = client;
        _logger = logger;
        _messenger = messenger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _messenger.RegisterAll(this);

        _client.Ready += OnClientReady;
        _client.Connected += OnClientConnected;
        _client.Log += OnClientLog;

        await _client.LoginAsync(TokenType.Bot, _options.Value.Token);
        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
    }

    private Task OnClientReady()
    {
        _logger.LogInformation("Ready on Discord");
        return Task.CompletedTask;
    }

    private Task OnClientConnected()
    {
        _logger.LogInformation("Connected to Discord");
        return Task.CompletedTask;
    }

    private Task OnClientLog(LogMessage arg)
    {
        _logger.Log(arg.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            _ => throw new ArgumentOutOfRangeException(nameof(arg), arg.Severity, null)
        }, arg.Exception, "{message}", arg.Message);

        return Task.CompletedTask;
    }

    public async void Receive(PostPublishedMessage message)
    {
        try
        {
            IEnumerable<Task> tasks =
                _options.Value.ChannelIds.Select(channelId => SendPostToChannel(message.Post, channelId));

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception in async void");
        }
    }

    private async Task SendPostToChannel(Post post, ulong channelId)
    {
        IChannel? channel = await _client.GetChannelAsync(channelId);

        if (channel is not IMessageChannel messageChannel)
        {
            _logger.LogWarning("{channelId} did not resolve to a valid message channel", channelId);
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle($"{post.Title} on {post.SourceWebsiteName}")
            .WithImageUrl(post.ImageUri)
            .WithCurrentTimestamp()
            .WithUrl(post.PostUri)
            .Build();

        await messageChannel.SendMessageAsync(embed: embed);
        _logger.LogInformation("Sent post {imageUri} to channel {channelId}", post.ImageUri, channelId);
    }
}

internal interface IDiscordService : IHostedService, IRecipient<PostPublishedMessage>;
