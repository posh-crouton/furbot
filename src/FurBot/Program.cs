using FurBot.Ioc;
using Microsoft.Extensions.Hosting;

namespace FurBot;

internal sealed class Program
{
    public static async Task<int> Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        FurBotServiceRegistrar.RegisterServices(builder);

        using IHost host = builder.Build();

        await host.RunAsync();

        return 0;
    }
}
