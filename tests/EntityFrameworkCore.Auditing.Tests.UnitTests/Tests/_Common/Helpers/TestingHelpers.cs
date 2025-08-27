// ReSharper disable once CheckNamespace

namespace EntityFrameworkCore.Auditing.Tests.UnitTests
{
    using Mango.Auditing;
    using Setup;
    using Setup.Fixture;

    public static class TestingHelpers
    {
        public static async Task<SampleEntity> CreateSampleEntityOnDb(BaseTestFixture fixture)
        {
            var entity = new SampleEntity
            {
                Name = "Initial Name",
                Description = "Initial Description"
            };

            await fixture.Context.SampleEntities.AddAsync(entity);
            await fixture.Context.SaveChangesAsync();
            return entity;
        }

        public static async Task UpdateSampleEntityOnDb(SampleEntity entity, BaseTestFixture fixture)
        {
            entity.Name = "Updated Name";
            entity.Description = "Updated Description";

            fixture.Context.SampleEntities.Update(entity);
            await fixture.Context.SaveChangesAsync(AuditLevel.Debug);
        }

        public static async Task DeleteSampleEntityOnDb(SampleEntity entity, BaseTestFixture fixture)
        {
            fixture.Context.SampleEntities.Remove(entity);
            await fixture.Context.SaveChangesAsync();
        }
    }
}