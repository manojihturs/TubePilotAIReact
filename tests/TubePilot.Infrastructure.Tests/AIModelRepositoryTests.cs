using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TubePilot.Domain.Entities;
using TubePilot.Infrastructure.Data;
using TubePilot.Infrastructure.Repositories;
using Xunit;

namespace TubePilot.Infrastructure.Tests
{
    public class AIModelRepositoryTests
    {
        private TubePilotDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TubePilotDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new TubePilotDbContext(options);
        }

        [Fact]
        public async Task AddAndGetById_Works()
        {
            using var db = CreateContext();
            var provider = new AIProvider { Id = Guid.NewGuid(), Name = "p", ProviderType = "x", CreatedBy = "t", CreatedDate = DateTime.UtcNow };
            db.AIProviders.Add(provider);
            await db.SaveChangesAsync();

            var repo = new AIModelRepository(db);
            var model = new AIModel { Id = Guid.NewGuid(), ProviderId = provider.Id, Name = "m1", CreatedDate = DateTime.UtcNow, IsEnabled = true };
            await repo.AddAsync(model);

            var fetched = await repo.GetByIdAsync(model.Id);
            Assert.NotNull(fetched);
            Assert.Equal("m1", fetched.Name);
            Assert.NotNull(fetched.Provider);
        }

        [Fact]
        public async Task ExistsByNameForProvider_Works()
        {
            using var db = CreateContext();
            var provider = new AIProvider { Id = Guid.NewGuid(), Name = "p2", ProviderType = "x", CreatedBy = "t", CreatedDate = DateTime.UtcNow };
            db.AIProviders.Add(provider);
            await db.SaveChangesAsync();

            var repo = new AIModelRepository(db);
            var m = new AIModel { Id = Guid.NewGuid(), ProviderId = provider.Id, Name = "dup", CreatedDate = DateTime.UtcNow };
            await repo.AddAsync(m);

            Assert.True(await repo.ExistsByNameForProviderAsync(provider.Id, "dup"));
            Assert.False(await repo.ExistsByNameForProviderAsync(provider.Id, "dup", m.Id));
        }

        [Fact]
        public async Task GetPaged_FiltersByProvider()
        {
            using var db = CreateContext();
            var p1 = new AIProvider { Id = Guid.NewGuid(), Name = "p1", ProviderType = "x", CreatedBy = "t", CreatedDate = DateTime.UtcNow };
            var p2 = new AIProvider { Id = Guid.NewGuid(), Name = "p2", ProviderType = "x", CreatedBy = "t", CreatedDate = DateTime.UtcNow };
            db.AIProviders.AddRange(p1, p2);
            await db.SaveChangesAsync();

            var repo = new AIModelRepository(db);
            for (int i = 0; i < 5; i++)
            {
                await repo.AddAsync(new AIModel { Id = Guid.NewGuid(), ProviderId = p1.Id, Name = "m" + i, CreatedDate = DateTime.UtcNow.AddMinutes(-i), IsEnabled = true });
            }
            for (int i = 0; i < 3; i++)
            {
                await repo.AddAsync(new AIModel { Id = Guid.NewGuid(), ProviderId = p2.Id, Name = "n" + i, CreatedDate = DateTime.UtcNow.AddMinutes(-i), IsEnabled = true });
            }

            var (items, total) = await repo.GetPagedAsync(1, 10, null, "name", "asc", p1.Id, null);
            Assert.Equal(5, total);
            Assert.Equal(5, items.Count());
        }
    }
}
