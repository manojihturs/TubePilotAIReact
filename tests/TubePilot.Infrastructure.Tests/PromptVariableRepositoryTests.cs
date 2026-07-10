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
    public class PromptVariableRepositoryTests
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
            var repo = new PromptVariableRepository(db);

            var p = new Prompt { Id = Guid.NewGuid(), Name = "P1", PromptText = "Hello", CreatedDate = DateTime.UtcNow };
            await db.Prompts.AddAsync(p);
            await db.SaveChangesAsync();

            var v = new PromptVariable { Id = Guid.NewGuid(), PromptId = p.Id, Name = "TOPIC", Placeholder = "{{TOPIC}}", CreatedDate = DateTime.UtcNow };
            await repo.AddAsync(v);
            await repo.SaveChangesAsync();

            var fetched = await repo.GetByIdAsync(v.Id);
            Assert.NotNull(fetched);
            Assert.Equal("TOPIC", fetched!.Name);
        }

        [Fact]
        public async Task GetPaged_FiltersAndSearch()
        {
            using var db = CreateContext();
            var repo = new PromptVariableRepository(db);

            var p1 = new Prompt { Id = Guid.NewGuid(), Name = "P1", PromptText = "Hello", CreatedDate = DateTime.UtcNow };
            var p2 = new Prompt { Id = Guid.NewGuid(), Name = "P2", PromptText = "World", CreatedDate = DateTime.UtcNow };
            await db.Prompts.AddAsync(p1);
            await db.Prompts.AddAsync(p2);
            await db.SaveChangesAsync();

            for (int i = 0; i < 12; i++)
            {
                var v = new PromptVariable { Id = Guid.NewGuid(), PromptId = i % 2 == 0 ? p1.Id : p2.Id, Name = "V" + i, Placeholder = "{{V" + i + "}}", CreatedDate = DateTime.UtcNow };
                await repo.AddAsync(v);
            }
            await repo.SaveChangesAsync();

            var (items, total) = await repo.GetPagedAsync(1, 10, p1.Id, "V1", "name", false);
            Assert.True(total >= 1);
            Assert.True(items.Any());
        }
    }
}
