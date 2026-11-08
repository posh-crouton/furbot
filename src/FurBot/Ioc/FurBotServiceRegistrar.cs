using CommunityToolkit.Mvvm.Messaging;
using Discord;
using Discord.WebSocket;
using FurBot.Clients;
using FurBot.Configuration;
using FurBot.Services;
using FurBot.Services.Polling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace FurBot.Ioc;

internal static class FurBotServiceRegistrar
{
    public static void RegisterServices(IHostApplicationBuilder builder)
    {
        builder.Configuration.AddUserSecrets<Properties.AssemblyInfo>();

        builder.Services.Configure<DiscordOptions>(
            builder.Configuration.GetSection(DiscordOptions.ConfigurationSectionName));
        builder.Services.Configure<GelbooruOptions>(
            builder.Configuration.GetSection(GelbooruOptions.ConfigurationSectionName));
        builder.Services.Configure<PollingServiceOptions>(
            builder.Configuration.GetSection(PollingServiceOptions.ConfigurationSectionName));

        var logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Information)
            .WriteTo.Console()
            .WriteTo.File(
                "logs/.log",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
        builder.Services.AddSingleton<Serilog.ILogger>(logger);
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(logger, dispose: false);
        });

        builder.Services.AddScoped<DiscordSocketClient>(_ => new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged
        }));

        builder.Services.AddHttpClient();
        builder.Services.AddScoped<IMessenger, WeakReferenceMessenger>();
        builder.Services.AddScoped<IGelbooruClient, GelbooruClient>();

        builder.Services.AddHostedService<DiscordService>();
        builder.Services.AddHostedService<GelbooruPollingService>();
    }
}
