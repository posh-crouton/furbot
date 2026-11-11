using FurBot.Database;
using Microsoft.Extensions.Hosting;

namespace FurBot.Services;

internal sealed class DatabaseInitializationService : IHostedService
{
    private readonly FurBotContext _dbContext;

    public DatabaseInitializationService(FurBotContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
