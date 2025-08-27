namespace EntityFrameworkCore.Auditing.Tests.UnitTests.Setup.Fixture
{
    using ActivitySource.Listeners;
    using Mango.Auditing;
    using Mango.Auditing.AuditableProperties;
    using Mango.Auditing.AuditableProperties.Contracts;
    using Mango.Auditing.AuditableProperties.Implementations;
    using Mango.Auditing.AuditLogging;
    using Mango.Auditing.Enrichers;
    using Mango.Auditing.Interceptors;
    using Mango.Auditing.Logging;
    using Mango.Auditing.Security;
    using Mango.Auditing.SoftDeletion;
    using Mango.Auditing.Telemetry;
    using global::Serilog;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Serilog.Sinks;
    using System.Diagnostics;
    using EnvironmentEnricher = Mango.Auditing.Enrichers.EnvironmentEnricher;
    using ILogger = global::Serilog.ILogger;
    using IPerformanceMonitor = Mango.Auditing.Telemetry.IPerformanceMonitor;
    using PerformanceMonitor = Mango.Auditing.Telemetry.PerformanceMonitor;
    using TraceIdentifierEnricher = Mango.Auditing.Enrichers.TraceIdentifierEnricher;

    public class BaseTestFixture : IAsyncLifetime
    {
        public TestDbContext Context { get; set; } = null!;
        public ICurrentUserProvider CurrentUserProvider { get; set; } = null!;
        public ServiceProvider ServiceProvider { get; set; } = null!;
        protected IServiceScopeFactory ScopeFactory { get; set; } = null!;
        public CustomInMemorySink InMemorySink { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            var services = new ServiceCollection();

            // 1) Create a brand-new sink instance for *this* fixture
            InMemorySink = new CustomInMemorySink();

            // 2) Configure the static Serilog logger to use *this* sink
            var serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Sink(InMemorySink)
                .CreateLogger();

            services.AddSingleton<ILogger>(serilogLogger);

            // Register Serilog with the built-in logging system.
            services.AddLogging(loggingBuilder =>
            {
                // Optionally clear other providers to ensure only Serilog is used.
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(serilogLogger, true);
            });

            // Register ActivitySource
            services.AddSingleton<ActivitySource>(new ActivitySource("DefaultActivityLogger"));

            // Register your test-specific implementations.
            services.AddSingleton<ILoggerContextProvider, DefaultLoggerContextProvider>();
            services.AddSingleton<IAuditLogger, DefaultAuditLogger>();

            // Telemetry specific services
            TestActivitySourceSetup.RegisterAllSampling("DefaultActivityLogger");

            services.AddSingleton<TelemetryOptions>(_ =>
            {
                var options = new TelemetryOptions()
                {
                    EnableDiagnostics = true,
                    MetricsEnabled = true,
                    MaxGlobalSamplesPerSecond = 1000,
                    MetricNamePrefix = "TestApp",
                };

                options.DefaultStrategy = new AlwaysSampleStrategy(options);
                return options;
            });
            services.AddSingleton<IPerformanceContextProvider, PerformanceContextProvider>();

            services.AddSingleton<IActivityLogger, DefaultActivityLogger>();
            services.AddSingleton<ITelemetryLogger, DefaultTelemetryLogger>();
            services.AddSingleton<IMetricProvider, DefaultMetricProvider>();
            services.AddSingleton<IAggregatedMetricProvider, DefaultAggregatedMetricProvider>();

            services.AddScoped<IMetricsMonitor, DefaultMetricsMonitor>();
            services.AddScoped<ITraceMonitor, DefaultTraceMonitor>();
            services.AddScoped<ILogMonitor, DefaultLogMonitor>();

            services.AddScoped<IPerformanceMonitor, PerformanceMonitor>();

            services.AddSingleton<IAggregationWindowStrategy>(_ => new RollingTimeWindowStrategy(TimeSpan.FromSeconds(1)));
            services.AddSingleton<IMetricSamplingStrategy, RoutingMetricSamplingStrategy>();
            services.AddSingleton<ILogger<RoutingMetricSamplingStrategy>>(sp =>
                sp.GetRequiredService<ILoggerFactory>().CreateLogger<RoutingMetricSamplingStrategy>());
            services.AddSingleton<IDiagnosticMetricReporter>(sp =>
            {
                var baseProvider = sp.GetRequiredService<IMetricProvider>(); // Not decorated
                var options = sp.GetRequiredService<TelemetryOptions>();

                return new DefaultDiagnosticMetricReporter(options, baseProvider);
            });
            services.AddSingleton<IRootMetricProvider, DiagnosticMetricProviderDecorator>();

            services.AddSingleton<ICurrentUserProvider, DefaultCurrentUserProvider>();
            services.AddSingleton<ISensitiveDataFilter, DefaultSensitiveDataFilter>();
            services.AddTransient<IAuditEnricher, EnvironmentEnricher>();
            services.AddTransient<IAuditEnricher, TraceIdentifierEnricher>();

            services.AddSingleton<IAuditablePropertiesStrategyDeterminant, DefaultAuditablePropertiesStrategyDeterminant>();
            services.AddSingleton<IAuditablePropertiesStrategyDispatcher, DefaultAuditablePropertiesStrategyDispatcher>();

            services.AddSingleton<ISoftDeletionStrategyDeterminant, DefaultSoftDeletionStrategyDeterminant>();
            services.AddSingleton<ISoftDeletionStrategyDispatcher, DefaultSoftDeletionStrategyDispatcher>();
            services.AddSingleton<ISoftDeletionMetricRecorder, DefaultSoftDeletionMetricRecorder>();
            services.AddSingleton<ISoftDeletionHandler, DefaultSoftDeletionHandler>();

            // Configure auditing options
            services.Configure<AuditingOptions>(options =>
            {
                options.TableConfiguration = new AuditTableConfiguration
                {
                    TableName = "AuditLogs",
                    Schema = "dbo"
                };

                options.Enrichment = new EnrichmentOptions
                {
                    IncludeUserInfo = true,
                    IncludeMetadata = true
                };

                options.Features.SetFeatureState(AuditFeature.Auditing);
                options.Features.SetFeatureState(AuditFeature.Enrichment);
                options.Features.SetFeatureState(AuditFeature.SoftDeletion);
                options.Features.SetFeatureState(AuditFeature.TrackUnmodified);
                options.Features.SetFeatureState(AuditFeature.IncludeEntityValues);
            });
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<AuditingOptions>>().Value);

            services.AddScoped<IAuditProvider, DefaultAuditProvider<TestDbContext>>();

            services.AddScoped<IAuditReader, DefaultAuditLogReader<TestDbContext>>();

            services.AddScoped<IAuditLogAttacher, DefaultAuditLogAttacher<TestDbContext>>();
            services.AddScoped<IAuditLogDetacher, DefaultAuditLogDetacher<TestDbContext>>();
            services.AddScoped<IAuditTracker, DefaultAuditLogTracker<TestDbContext>>();

            services.AddScoped<IAuditLogCreator, AuditLogCreator>();

            services.AddScoped<IInterceptor, AuditLoggingInterceptor>();
            services.AddScoped<IInterceptor, SoftDeletionInterceptor>();
            services.AddScoped<IInterceptor, AuditablePropertiesInterceptor>();

            // Configure the DbContext with an in-memory provider
            services.AddDbContext<TestDbContext>((serviceProvider, optionsBuilder) =>
            {
                optionsBuilder.UseInMemoryDatabase($"TestAuditingDb_{Guid.NewGuid()}");
                optionsBuilder
                    .AddInterceptors(
                        serviceProvider.GetServices<IInterceptor>()
                    );
            });

            services.AddScoped<Lazy<TestDbContext>>(sp =>
                new Lazy<TestDbContext>(sp.GetRequiredService<TestDbContext>));

            // Build the service provider and capture the scope factory for creating scoped services in tests.
            ServiceProvider = services.BuildServiceProvider();
            ScopeFactory = ServiceProvider.GetRequiredService<IServiceScopeFactory>();

            Context = ServiceProvider.GetRequiredService<TestDbContext>();
            CurrentUserProvider = ServiceProvider.GetRequiredService<ICurrentUserProvider>();

            await Context.Database.EnsureCreatedAsync();
            await SeedData();
        }

        public Task DisposeAsync()
        {
            if (ServiceProvider is IDisposable disposable) disposable.Dispose();
            return Task.CompletedTask;
        }

        public void SetTestUserContext()
        {
            DefaultCurrentUserProvider.SetCurrentUser(Guid.NewGuid().ToString(), "TestUser",
                new Dictionary<string, string>
                {
                    { "Key1", "Value1" },
                    { "Key2", "Value2" }
                });
        }

        public T GetService<T>() where T : notnull
        {
            using var scope = ScopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<T>();
        }

        private async Task SeedData()
        {
            // Seed your database with initial data if needed
            var entities = new List<SampleEntity>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Entity 1",
                    Description = "This is a sample entity.",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "TestUser"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Entity 2",
                    Description = "This is a sample entity.",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "TestUser"
                }
            };

            Context.SampleEntities.AddRange(entities);
            await Context.SaveChangesAsync(CancellationToken.None);
        }
    }
}