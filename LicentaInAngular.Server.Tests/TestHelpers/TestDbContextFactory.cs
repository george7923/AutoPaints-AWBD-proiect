using LicentaInAngular.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace LicentaInAngular.Server.Tests.TestHelpers
{
    public static class TestDbContextFactory
    {
        public static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            return context;
        }
    }
}
