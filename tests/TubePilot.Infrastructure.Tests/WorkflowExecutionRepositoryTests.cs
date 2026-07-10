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
    public class WorkflowExecutionRepositoryTests
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
            var wf = new Workflow { Id = Guid.NewGuid(), Name = "wf1", CreatedDate = DateTime.UtcNow };
            db.Workflows.Add(wf);
            await db.SaveChangesAsync();

            var repo = new WorkflowExecutionRepository(db);
            var exec = new WorkflowExecution { Id = Guid.NewGuid(), WorkflowId = wf.Id, CurrentStep = 0, Status = "Running", InputJson = "{}", StartedAt = DateTime.UtcNow };
            await repo.AddAsync(exec);

            var fetched = await repo.GetByIdAsync(exec.Id);
            Assert.NotNull(fetched);
            Assert.Equal("Running", fetched.Status);
        }
    }
}
