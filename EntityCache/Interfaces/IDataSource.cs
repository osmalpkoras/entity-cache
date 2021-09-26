using Microsoft.EntityFrameworkCore;

namespace EntityCache.Interfaces
{
    public interface IDataSource
    {
        bool Exists { get; }

        bool DeployDatabase();

        bool SaveChanges(DbContext context);

        DbContext CreateDbContext();
    }
}
