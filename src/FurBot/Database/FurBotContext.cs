using FurBot.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace FurBot.Database;

internal sealed class FurBotContext : DbContext, IFurbotContext
{
    public FurBotContext(DbContextOptions<FurBotContext> options) : base(options) { }

    public DbSet<PostEntity> Posts => Set<PostEntity>();
}

internal interface IFurbotContext
{
    DbSet<PostEntity> Posts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
