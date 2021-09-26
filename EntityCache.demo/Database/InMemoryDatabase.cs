using EntityCache.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace EntityCache.demo.Database
{
    public class InMemoryDatabase : IDataSource
    {
        public bool Exists => true;

        public DbContext CreateDbContext()
        {
            return new DemoDbContext(CreateOptions<DemoDbContext>());
        }

        public bool DeployDatabase() => true;

        public bool SaveChanges(DbContext context)
        {
            if (!context.ChangeTracker.HasChanges())
            {
                return true;
            }
            bool saveFailed;
            int maxTrys = 10;
            int i = 0;
            do
            {
                i++;
                saveFailed = false;
                try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    // Update original values from the database
                    foreach (EntityEntry entry in ex.Entries)
                    {
                        entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                    }
                }

            } while (saveFailed && i <= maxTrys);

            return !saveFailed;
        }

        public static DbContextOptions CreateOptions<T>() where T : DbContext
        {
            //This creates the SQLite connection string to in-memory database
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            { DataSource = ":memory:" };
            var connectionString = connectionStringBuilder.ToString();

            //This creates a SqliteConnectionwith that string
            var connection = new SqliteConnection(connectionString);

            //The connection MUST be opened here
            connection.Open();

            //Now we have the EF Core commands to create SQLite options
            var builder = new DbContextOptionsBuilder<T>();
            builder.UseSqlite(connection);

            return builder.Options;
        }
    }
}
