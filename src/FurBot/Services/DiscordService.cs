using Discord;
using Discord.WebSocket;
using FurBot.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FurBot.Services;

internal sealed class DiscordService : IDiscordService
{
    private readonly IOptions<DiscordOptions> _options;
    private readonly DiscordSocketClient _client;
    private readonly ILogger<DiscordService> _logger;

    public DiscordService(IOptions<DiscordOptions> options,
        DiscordSocketClient client,
        ILogger<DiscordService> logger)
    {
        _options = options;
        _client = client;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Log += OnClientLog;

        await _client.LoginAsync(TokenType.Bot, _options.Value.Token);
        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
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
}

internal interface IDiscordService : IHostedService;
