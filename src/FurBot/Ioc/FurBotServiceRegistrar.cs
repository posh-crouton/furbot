using CommunityToolkit.Mvvm.Messaging;
using Discord;
using Discord.WebSocket;
using FurBot.Clients;
using FurBot.Configuration;
using FurBot.Database;
using FurBot.Enums;
using FurBot.Services;
using FurBot.Services.Polling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
        builder.Services.Configure<FurBotContextOptions>(
            builder.Configuration.GetSection(FurBotContextOptions.ConfigurationSectionName));
        builder.Services.AddDbContext<FurBotContext>((services, options) =>
        {
            var database = services.GetRequiredService<IOptions<FurBotContextOptions>>().Value;
            switch (database.Provider)
            {
                case DatabaseProvider.Sqlite:
                    options.UseSqlite(database.ConnectionString);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported database provider: {database.Provider}");
            }
        }, contextLifetime: ServiceLifetime.Singleton, optionsLifetime: ServiceLifetime.Singleton);
        builder.Services.AddSingleton<IFurbotContext>(services => services.GetRequiredService<FurBotContext>());

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

        builder.Services.AddSingleton<DiscordSocketClient>(_ => new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged
        }));

        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();
        builder.Services.AddTransient<IGelbooruClient, GelbooruClient>();

        builder.Services.Configure<HostOptions>(options => options.ServicesStartConcurrently = false);
        builder.Services.AddHostedService<DatabaseInitializationService>();
        builder.Services.AddHostedService<DiscordService>();
        builder.Services.AddHostedService<GelbooruPollingService>();
    }
}
