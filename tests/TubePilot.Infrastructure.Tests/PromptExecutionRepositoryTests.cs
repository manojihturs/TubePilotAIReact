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
    public class PromptExecutionRepositoryTests
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
            var prompt = new Prompt { Id = Guid.NewGuid(), Name = "t", Description = "c", PromptText = "c" };
            db.Prompts.Add(prompt);
            await db.SaveChangesAsync();

            var repo = new PromptExecutionRepository(db);
            var exec = new PromptExecution { Id = Guid.NewGuid(), PromptId = prompt.Id, RenderedPrompt = "r", CreatedDate = DateTime.UtcNow };
            await repo.AddAsync(exec);

            var fetched = await repo.GetByIdAsync(exec.Id);
            Assert.NotNull(fetched);
            Assert.Equal(exec.RenderedPrompt, fetched.RenderedPrompt);
        }

        [Fact]
        public async Task GetPaged_FiltersByPrompt()
        {
            using var db = CreateContext();
            var p = new Prompt { Id = Guid.NewGuid(), Name = "p1", Description = "c", PromptText = "c" };
            db.Prompts.Add(p);
            await db.SaveChangesAsync();

            var repo = new PromptExecutionRepository(db);
            for (int i = 0; i < 5; i++)
            {
                await repo.AddAsync(new PromptExecution { Id = Guid.NewGuid(), PromptId = p.Id, RenderedPrompt = "r" + i, CreatedDate = DateTime.UtcNow.AddMinutes(-i) });
            }

            var (items, total) = await repo.GetPagedAsync(1, 10, p.Id, null, null, null);
            Assert.Equal(5, total);
            Assert.Equal(5, items.Count());
        }
    }
}
