using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TubePilot.Infrastructure.Data;
using TubePilot.Infrastructure.Repositories;
using TubePilot.Domain.Entities;
using Xunit;
using System.Linq;

namespace TubePilot.Infrastructure.Tests
{
    public class AIProviderRepositoryTests
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
            var repo = new AIProviderRepository(db);

            var entity = new AIProvider
            {
                Id = Guid.NewGuid(),
                Name = "test",
                ProviderType = "OpenAI",
                CreatedBy = "test",
                CreatedDate = DateTime.UtcNow
            };

            await repo.AddAsync(entity);

            var fetched = await repo.GetByIdAsync(entity.Id);
            Assert.NotNull(fetched);
            Assert.Equal("test", fetched.Name);
        }

        [Fact]
        public async Task ExistsByName_PreventsDuplicates()
        {
            using var db = CreateContext();
            var repo = new AIProviderRepository(db);

            var e1 = new AIProvider { Id = Guid.NewGuid(), Name = "dup", ProviderType = "x", CreatedBy = "t", CreatedDate = DateTime.UtcNow };
            await repo.AddAsync(e1);

            var exists = await repo.ExistsByNameAsync("dup");
            Assert.True(exists);

            var existsExcluding = await repo.ExistsByNameAsync("dup", e1.Id);
            Assert.False(existsExcluding);
        }

        [Fact]
        public async Task GetPaged_Returns_CorrectPaging()
        {
            using var db = CreateContext();
            var repo = new AIProviderRepository(db);

            for (int i = 0; i < 15; i++)
            {
                await repo.AddAsync(new AIProvider { Id = Guid.NewGuid(), Name = "p" + i, ProviderType = "t", CreatedBy = "t", CreatedDate = DateTime.UtcNow.AddMinutes(-i) });
            }

            var (items, total) = await repo.GetPagedAsync(1, 10, null, "name", "asc", null);
            Assert.Equal(15, total);
            Assert.Equal(10, items.Count());
        }
    }
}
