// ReSharper disable once CheckNamespace
namespace EntityFrameworkCore.Auditing.Tests.UnitTests
{
    using Mango.Auditing;
    using Mango.Auditing.Auditing.Enrichers.Models;
    using Mango.Auditing.Enrichers;
    using FluentAssertions;
    using Setup.Fixture;

    public class AuditLogExtensionsTests(BaseTestFixture fixture) : IClassFixture<BaseTestFixture>
    {
        [Fact]
        public async Task EnrichAuditLog_Should_Enrich_AuditLog_Synchronously()
        {
            // Arrange
            var auditLog = new AuditLog();

            var enrichers = new List<IAuditEnricher> { new TestAuditEnricher() };
            var serviceProvider = fixture.ServiceProvider;

            var sampleEntity = await TestingHelpers.CreateSampleEntityOnDb(fixture);
            var entry = fixture.Context.Entry(sampleEntity);

            // Act
            auditLog.EnrichAuditLog(entry, enrichers, serviceProvider);

            // Assert
            auditLog.Metadata.Should().ContainKey("TestEnricher").WhoseValue.Should().Be("Synchronous enrichment");
        }

        [Fact]
        public async Task EnrichAuditLog_Should_Add_ExceptionEntry_When_Synchronous_Enricher_Fails()
        {
            // Arrange
            var auditLog = new AuditLog();
            var enrichers = new List<IAuditEnricher> { new TestExceptionAuditEnricher() };
            var serviceProvider = fixture.ServiceProvider;
            var sampleEntity = await TestingHelpers.CreateSampleEntityOnDb(fixture);
            var entry = fixture.Context.Entry(sampleEntity);

            // Act
            auditLog.EnrichAuditLog(entry, enrichers, serviceProvider);

            // Assert
            auditLog.Metadata.Should().ContainKey("EnricherError_TestExceptionAuditEnricher")
                .WhoseValue.Should().Be("Test exception in synchronous enricher");
        }

        [Fact]
        public async Task EnrichAuditLogAsync_Should_Enrich_AuditLog_Synchronously()
        {
            // Arrange
            var auditLog = new AuditLog();

            var enrichers = new List<IAuditEnricher> { new TestAuditEnricher() };
            var serviceProvider = fixture.ServiceProvider;

            var sampleEntity = await TestingHelpers.CreateSampleEntityOnDb(fixture);
            var entry = fixture.Context.Entry(sampleEntity);

            // Act
            await auditLog.EnrichAuditLogAsync(entry, enrichers, serviceProvider, CancellationToken.None);

            // Assert
            auditLog.Metadata.Should().ContainKey("TestEnricherAsync").WhoseValue.Should().Be("Asynchronous enrichment");
        }

        [Fact]
        public async Task EnrichAuditLogAsync_Should_Add_ExceptionEntry_When_Synchronous_Enricher_Fails()
        {
            // Arrange
            var auditLog = new AuditLog();
            var enrichers = new List<IAuditEnricher> { new TestExceptionAuditEnricher() };
            var serviceProvider = fixture.ServiceProvider;
            var sampleEntity = await TestingHelpers.CreateSampleEntityOnDb(fixture);
            var entry = fixture.Context.Entry(sampleEntity);

            // Act
            await auditLog.EnrichAuditLogAsync(entry, enrichers, serviceProvider, CancellationToken.None);

            // Assert
            auditLog.Metadata.Should().ContainKey("EnricherError_TestExceptionAuditEnricher")
                .WhoseValue.Should().Be("Test exception in asynchronous enricher");
        }

        [Fact]
        public async Task GetChangesAsync_Should_Return_Changes_When_OldAndNewValues_Are_Present()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                OldValues = "{\"Name\":\"OldName\",\"Age\":30}",
                NewValues = "{\"Name\":\"NewName\",\"Age\":31}",
                AffectedColumns = "Name,Age"
            };

            // Act
            var changes = await auditLog.GetChangesAsync();

            // Assert
            changes.Should().HaveCount(2);
            changes.Should().Contain(c => c.PropertyName == "Name" && c.OriginalValue!.ToString() == "OldName" && c.NewValue!.ToString() == "NewName");
            changes.Should().Contain(c => c.PropertyName == "Age" && c.OriginalValue!.ToString() == "30" && c.NewValue!.ToString() == "31");
        }

        [Fact]
        public async Task GetChangesAsync_Should_Return_Empty_When_OldAndNewValues_Are_Empty()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                OldValues = null,
                NewValues = null,
                AffectedColumns = "Name,Age"
            };
            // Act
            var changes = await auditLog.GetChangesAsync();

            // Assert
            changes.Should().BeEmpty();
        }

        [Fact]
        public async Task GetChangesAsync_Should_Return_Empty_When_AffectedColumns_Are_Empty()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                OldValues = "{\"Name\":\"OldName\",\"Age\":30}",
                NewValues = "{\"Name\":\"NewName\",\"Age\":31}",
                AffectedColumns = null
            };
            // Act
            var changes = await auditLog.GetChangesAsync();

            // Assert
            changes.Should().BeEmpty();
        }

        [Fact]
        public async Task GetChangesAsync_Should_Return_Sensitive_If_MaskedValues_Present()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                OldValues = "{\"Username\":\"OldUsername\",\"Password\":\"***MASKED***\"}",
                NewValues = "{\"Username\":\"NewUsername\",\"Password\":\"***MASKED***\"}",
                AffectedColumns = "Username,Password"
            };
            // Act
            var changes = await auditLog.GetChangesAsync();

            // Assert
            changes.Should().HaveCount(2);
            changes.Should().Contain(c => c.PropertyName == "Username" && c.OriginalValue!.ToString() == "OldUsername" && c.NewValue!.ToString() == "NewUsername");
            changes.Should().Contain(c => c.PropertyName == "Password" && c.OriginalValue!.ToString() == "***MASKED***" && c.NewValue!.ToString() == "***MASKED***");
            changes.Where(c => c.PropertyName == "Password").Should().OnlyContain(c => c.IsSensitive == true);
        }


        [Fact]
        public async Task GetMetadataAsync_Should_Return_Metadata_When_Present()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                Metadata = new Dictionary<string, string>
                {
                    { "Key1", "Value1" },
                    { "Key2", "Value2" }
                }
            };
            // Act
            var metadata = await auditLog.GetMetadataAsync();

            // Assert
            metadata.Should().ContainKey("Key1").WhoseValue.Should().Be("Value1");
            metadata.Should().ContainKey("Key2").WhoseValue.Should().Be("Value2");
        }

        [Fact]
        public async Task GetMetadataAsync_Should_Return_Empty_When_No_Metadata_Present()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                Metadata = new Dictionary<string, string>()
            };
            // Act
            var metadata = await auditLog.GetMetadataAsync();

            // Assert
            metadata.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMetadataAsync_Should_Return_Empty_When_Null()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                Metadata = default!
            };
            // Act
            var metadata = await auditLog.GetMetadataAsync();

            // Assert
            metadata.Should().BeEmpty();
        }

        private class TestAuditEnricher : IAuditEnricher
        {
            public int Order => 0;
            public void Enrich(AuditLog auditLog, EnrichmentContext context)
            {
                // Example implementation
                auditLog.Metadata["TestEnricher"] = "Synchronous enrichment";
            }
            public Task EnrichAsync(AuditLog auditLog, EnrichmentContext context)
            {
                // Example implementation
                auditLog.Metadata["TestEnricherAsync"] = "Asynchronous enrichment";
                return Task.CompletedTask;
            }
        }

        private class TestExceptionAuditEnricher : IAuditEnricher
        {
            public int Order => 0;
            public void Enrich(AuditLog auditLog, EnrichmentContext context) => throw new InvalidOperationException("Test exception in synchronous enricher");
            public Task EnrichAsync(AuditLog auditLog, EnrichmentContext context) => throw new InvalidOperationException("Test exception in asynchronous enricher");
        }
    }
}
