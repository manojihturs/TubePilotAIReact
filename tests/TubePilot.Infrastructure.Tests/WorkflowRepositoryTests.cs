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
    public class WorkflowRepositoryTests
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
            var repo = new WorkflowRepository(db);

            var wf = new Workflow { Id = Guid.NewGuid(), Name = "Movie Workflow", Description = "Test", CreatedDate = DateTime.UtcNow };
            wf.Steps.Add(new WorkflowStep{ Id = Guid.NewGuid(), Name = "Research", Order = 1, Type = "Research" });
            wf.Steps.Add(new WorkflowStep{ Id = Guid.NewGuid(), Name = "Render", Order = 2, Type = "Render" });

            await repo.AddAsync(wf);

            var fetched = await repo.GetByIdAsync(wf.Id);
            Assert.NotNull(fetched);
            Assert.Equal(2, fetched.Steps.Count);
        }

        [Fact]
        public async Task GetPaged_Returns_Items()
        {
            using var db = CreateContext();
            var repo = new WorkflowRepository(db);
            for (int i = 0; i < 5; i++)
            {
                await repo.AddAsync(new Workflow { Id = Guid.NewGuid(), Name = "wf" + i, CreatedDate = DateTime.UtcNow });
            }

            var (items, total) = await repo.GetPagedAsync(1, 10, null);
            Assert.Equal(5, total);
            Assert.Equal(5, items.Count());
        }
    }
}
