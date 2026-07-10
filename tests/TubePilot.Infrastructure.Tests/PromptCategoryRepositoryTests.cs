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
    public class PromptCategoryRepositoryTests
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
            var repo = new PromptCategoryRepository(db);

            var entity = new PromptCategory { Id = Guid.NewGuid(), Name = "Test", CreatedDate = DateTime.UtcNow };
            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();

            var fetched = await repo.GetByIdAsync(entity.Id);
            Assert.NotNull(fetched);
            Assert.Equal("Test", fetched!.Name);
        }

        [Fact]
        public async Task GetAll_Excludes_SoftDeleted()
        {
            using var db = CreateContext();
            var repo = new PromptCategoryRepository(db);

            var a = new PromptCategory { Id = Guid.NewGuid(), Name = "A", CreatedDate = DateTime.UtcNow };
            var b = new PromptCategory { Id = Guid.NewGuid(), Name = "B", CreatedDate = DateTime.UtcNow, SoftDelete = true };
            await repo.AddAsync(a);
            await repo.AddAsync(b);
            await repo.SaveChangesAsync();

            var list = (await repo.GetAllAsync()).ToList();
            Assert.Single(list);
            Assert.Equal("A", list[0].Name);
        }

        [Fact]
        public async Task Update_Works()
        {
            using var db = CreateContext();
            var repo = new PromptCategoryRepository(db);

            var entity = new PromptCategory { Id = Guid.NewGuid(), Name = "Old", CreatedDate = DateTime.UtcNow };
            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();

            entity.Name = "New";
            await repo.UpdateAsync(entity);
            await repo.SaveChangesAsync();

            var fetched = await repo.GetByIdAsync(entity.Id);
            Assert.Equal("New", fetched!.Name);
        }

        [Fact]
        public async Task Delete_Sets_SoftDelete()
        {
            using var db = CreateContext();
            var repo = new PromptCategoryRepository(db);

            var entity = new PromptCategory { Id = Guid.NewGuid(), Name = "ToDelete", CreatedDate = DateTime.UtcNow };
            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();

            await repo.DeleteAsync(entity.Id);
            await repo.SaveChangesAsync();

            var fetched = await repo.GetByIdAsync(entity.Id);
            Assert.Null(fetched);

            var all = await db.PromptCategories.IgnoreQueryFilters().ToListAsync();
            Assert.Single(all);
            Assert.True(all[0].SoftDelete);
        }
    }
}
