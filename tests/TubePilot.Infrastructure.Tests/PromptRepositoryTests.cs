using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using TubePilot.Domain.Entities;
using TubePilot.Infrastructure.Data;
using TubePilot.Infrastructure.Repositories;

namespace TubePilot.Infrastructure.Tests
{
    public class PromptRepositoryTests
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
            var repo = new PromptRepository(db);

            var pc = new PromptCategory { Id = Guid.NewGuid(), Name = "Cat", CreatedDate = DateTime.UtcNow };
            await db.PromptCategories.AddAsync(pc);
            await db.SaveChangesAsync();

            var p = new Prompt { Id = Guid.NewGuid(), CategoryId = pc.Id, Name = "TestPrompt", PromptText = "Hello" , CreatedDate = DateTime.UtcNow };
            await repo.AddAsync(p);
            await repo.SaveChangesAsync();

            var fetched = await repo.GetByIdAsync(p.Id);
            Assert.NotNull(fetched);
            Assert.Equal("TestPrompt", fetched!.Name);
        }

        [Fact]
        public async Task GetPaged_FiltersAndSearch()
        {
            using var db = CreateContext();
            var repo = new PromptRepository(db);

            var cat1 = new PromptCategory { Id = Guid.NewGuid(), Name = "C1", CreatedDate = DateTime.UtcNow };
            var cat2 = new PromptCategory { Id = Guid.NewGuid(), Name = "C2", CreatedDate = DateTime.UtcNow };
            await db.PromptCategories.AddAsync(cat1);
            await db.PromptCategories.AddAsync(cat2);
            await db.SaveChangesAsync();

            for (int i = 0; i < 15; i++)
            {
                var p = new Prompt { Id = Guid.NewGuid(), CategoryId = i % 2 == 0 ? cat1.Id : cat2.Id, Name = "P" + i, PromptText = "Text " + i, CreatedDate = DateTime.UtcNow };
                await repo.AddAsync(p);
            }
            await repo.SaveChangesAsync();

            var (items, total) = await repo.GetPagedAsync(1, 10, cat1.Id, "P1", "name", false);
            Assert.True(total >= 1);
            Assert.True(items.Any());
        }
    }
}
