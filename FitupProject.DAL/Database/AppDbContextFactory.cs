using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FitupProject.DAL.Database
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var root = FindSolutionRoot();
            var appsettings = Path.Combine(root, "FitupProject", "appsettings.json");
            var appsettingsDev = Path.Combine(root, "FitupProject", "appsettings.Development.json");

            if (!File.Exists(appsettings))
                throw new InvalidOperationException($"Cannot find appsettings at: {appsettings}");

            var config = new ConfigurationBuilder()
                .AddJsonFile(appsettings, optional: false, reloadOnChange: false)
                .AddJsonFile(appsettingsDev, optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var cs = config.GetConnectionString("NeonDb");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Missing ConnectionStrings:NeonDb in FitupProject/appsettings.json");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(cs);

            return new AppDbContext(optionsBuilder.Options);
        }

        private static string FindSolutionRoot()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (dir != null)
            {
                // root solution thường chứa folder FitupProject
                if (Directory.Exists(Path.Combine(dir.FullName, "FitupProject")))
                    return dir.FullName;

                dir = dir.Parent;
            }

            throw new InvalidOperationException("Cannot locate solution root containing 'FitupProject' folder.");
        }
    }
}
