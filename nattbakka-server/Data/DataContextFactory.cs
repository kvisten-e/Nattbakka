using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace nattbakka_server.Data
{
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<DataContext>();
            DotNetEnv.Env.Load(); 
            var connectionString = Environment.GetEnvironmentVariable("MyDb_ConnectionString")
                                   ?? throw new InvalidOperationException("MyDb_ConnectionString not configured");
            
            builder.UseSqlServer(connectionString);

            return new DataContext(builder.Options);
        }
    }
}