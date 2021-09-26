using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EntityCache.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EntityCache.demo.Database
{
    public class AccessDatabase : IDataSource
    {
        public bool Exists
        {
            get
            {
                using var context = CreateDbContext();
                return context.Database.CanConnect();
            }
        }
        public bool DeployDatabase()
        {
            using var context = CreateDbContext();
            context.Database.EnsureCreated();
            context.Database.Migrate();
            SaveChanges(context);
            return true;
        }

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

        public DbContext CreateDbContext()
        {
            return new DemoDbContext(CreateOptions<DemoDbContext>());
        }
        public static DbContextOptions CreateOptions<T>() where T : DbContext
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "myAccessFile.accdb");
            var builder = new DbContextOptionsBuilder<T>();
            builder.UseJet(@$"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};");

            return builder.Options;
        }
    }
}
