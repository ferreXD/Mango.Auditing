// ReSharper disable once CheckNamespace
namespace EntityFrameworkCore.Auditing.Tests.UnitTests
{
    using Mango.Auditing;
    using Mango.Auditing.Security;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Setup;
    using Setup.Fixture;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class EntityEntryExtensionsTests(BaseTestFixture fixture) : IClassFixture<BaseTestFixture>
    {
        [Fact]
        public async Task GetAffectedColumns_NoChanges_ReturnsEmpty()
        {
            var entity = await TestingHelpers.CreateSampleEntityOnDb(fixture);
            var entry = fixture.Context.Entry(entity);

            // Nothing modified
            entry.State = EntityState.Unchanged;
            entry.GetAffectedColumns().Should().BeEmpty();
        }

        [Fact]
        public async Task GetEntityId_Returns_PrimaryKey_AsString()
        {
            var entity = await TestingHelpers.CreateSampleEntityOnDb(fixture);

            var id = fixture.Context.Entry(entity).GetEntityId();
            id.Should().Be(entity.Id.ToString());
        }

        [Fact]
        public async Task GetPrimaryKey_Formats_KeyNameAndValue()
        {
            // no need to hit DB for this one
            var entity = await TestingHelpers.CreateSampleEntityOnDb(fixture);
            var entry = fixture.Context.Entry(entity);

            var pk = entry.GetPrimaryKey();
            pk.Should().Be($"{nameof(SampleEntity.Id)}={entity.Id}");
        }

        [Fact]
        public async Task GetAffectedColumns_List_ModifiedProperties()
        {
            var context = fixture.Context;
            var entity = await TestingHelpers.CreateSampleEntityOnDb(fixture);

            entity.Name = "newName";
            context.Update(entity);

            var entry = fixture.Context.Entry(entity);

            var affected = entry.GetAffectedColumns();
            affected.Should().Contain(nameof(SampleEntity.Name));
        }

        [Fact]
        public void GetOldValues_And_NewValues_RespectIncludeFlag_And_FilterSensitive()
        {
            var ctx = fixture.Context;

            // seed & attach
            var e = new SampleEntity { Id = Guid.NewGuid(), Name = "origN", Description = "origD" };
            ctx.SampleEntities.Add(e);
            ctx.SaveChanges();

            // simulate modification
            var entry = ctx.Entry(e);
            entry.Property(nameof(SampleEntity.Name)).OriginalValue = "origN";
            entry.Property(nameof(SampleEntity.Name)).CurrentValue = "newN";
            entry.Property(nameof(SampleEntity.Name)).IsModified = true;

            entry.Property(nameof(SampleEntity.Description)).OriginalValue = "origD";
            entry.Property(nameof(SampleEntity.Description)).CurrentValue = "newD";
            entry.Property(nameof(SampleEntity.Description)).IsModified = true;

            // mask Description
            var filter = new DummyFilter(
                isSensitive: (type, prop) => prop == nameof(SampleEntity.Description),
                mask: v => "[MASKED]"
            );

            // when includeEntityValues = false => null
            entry.GetOldValues(filter, includeEntityValues: false).Should().BeNull();
            entry.GetNewValues(filter, includeEntityValues: false).Should().BeNull();

            // include = true => JSON with exactly two keys
            var oldDict = JsonSerializer
                .Deserialize<Dictionary<string, object?>>(
                    entry.GetOldValues(filter, includeEntityValues: true)!)!;

            oldDict.Keys.Should().BeEquivalentTo(new[] { "Name", "Description" });
            oldDict["Name"]!.ToString().Should().Be("origN");
            oldDict["Description"]!.ToString().Should().Be("[MASKED]");

            var newDict = JsonSerializer
                .Deserialize<Dictionary<string, object?>>(
                    entry.GetNewValues(filter, includeEntityValues: true)!)!;

            newDict.Keys.Should().BeEquivalentTo(new[] { "Name", "Description" });
            newDict["Name"]!.ToString().Should().Be("newN");
            newDict["Description"]!.ToString().Should().Be("[MASKED]");
        }

        // Simple stub for ISensitiveDataFilter
        private class DummyFilter(Func<string, string, bool> isSensitive, Func<object?, object?> mask)
            : ISensitiveDataFilter
        {
            public bool IsSensitive(string entityType, string propertyName) => isSensitive(entityType, propertyName);
            public object MaskValue(object? value) => mask(value)!;
        }
    }
}
