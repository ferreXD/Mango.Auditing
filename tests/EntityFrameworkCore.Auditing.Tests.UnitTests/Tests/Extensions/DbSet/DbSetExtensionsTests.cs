// ReSharper disable once CheckNamespace
namespace EntityFrameworkCore.Auditing.Tests.UnitTests
{
    using Mango.Auditing;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Setup.Fixture;
    using System.Linq;
    using System.Threading.Tasks;

    [CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
    public class DbSetExtensionsTests : IClassFixture<BaseTestFixture>
    {
        private readonly BaseTestFixture _fixture;

        public DbSetExtensionsTests(BaseTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Default_Query_Should_Exclude_Deleted_Entities()
        {
            // Arrange: create two, then soft-delete the second
            var keep = await TestingHelpers.CreateSampleEntityOnDb(_fixture);
            var del = await TestingHelpers.CreateSampleEntityOnDb(_fixture);
            await TestingHelpers.DeleteSampleEntityOnDb(del, _fixture);

            // Act
            var live = await _fixture.Context.SampleEntities.ToListAsync();

            // Assert
            live.Select(e => e.Id).Should().Contain(keep.Id);
            live.Select(e => e.Id).Should().NotContain(del.Id);
        }

        [Fact]
        public async Task IncludeSoftDeleted_Should_Return_All_Entities()
        {
            // Arrange
            var keep = await TestingHelpers.CreateSampleEntityOnDb(_fixture);
            var del = await TestingHelpers.CreateSampleEntityOnDb(_fixture);
            await TestingHelpers.DeleteSampleEntityOnDb(del, _fixture);

            // Act
            var all = await _fixture.Context
                                     .SampleEntities
                                     .IncludeSoftDeleted()
                                     .ToListAsync();

            // Assert
            all.Select(e => e.Id)
               .Should().Contain(new[] { keep.Id, del.Id });
        }

        [Fact]
        public async Task OnlySoftDeleted_Should_Return_Only_Deleted_Entities()
        {
            // Arrange
            var keep = await TestingHelpers.CreateSampleEntityOnDb(_fixture);
            var del = await TestingHelpers.CreateSampleEntityOnDb(_fixture);
            await TestingHelpers.DeleteSampleEntityOnDb(del, _fixture);

            // Act
            var onlyDeleted = await _fixture.Context
                                            .SampleEntities
                                            .OnlySoftDeleted()
                                            .ToListAsync();

            // Assert
            onlyDeleted.Select(e => e.Id)
                       .Should().Contain(del.Id)
                       .And.NotContain(keep.Id);
        }
    }
}
