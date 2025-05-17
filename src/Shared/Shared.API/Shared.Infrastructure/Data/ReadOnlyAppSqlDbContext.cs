
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Shared.Infrastructure.Data
{
    public class ReadOnlyAppSqlDbContext : AppSqlDbContext
    {

        public ReadOnlyAppSqlDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // we dont need tracking, since it is all read
        }

        public override int SaveChanges()
        {
            throw new InvalidOperationException("Read-only context: SaveChanges is disabled.");
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Read-only context: SaveChangesAsync is disabled.");
        }

        public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
        {
            throw new InvalidOperationException("Read-only context: Add is disabled.");
        }

        public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        {
            throw new InvalidOperationException("Read-only context: Update is disabled.");
        }

        public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
        {
            throw new InvalidOperationException("Read-only context: Remove is disabled.");
        }

        public override void AddRange(params object[] entities)
        {
            throw new InvalidOperationException("Read-only context: AddRange is disabled.");
        }
    }
}
