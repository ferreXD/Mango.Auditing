# Mango.Auditing

> **Auditing for .NET + EF Core** with optional telemetry hooks. Capture who/what/when/where, persist it, and (optionally) emit traces/metrics/logs.

[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Last commit](https://img.shields.io/github/last-commit/ferreXD/Mango.Auditing.svg)](./)

> **Status:** Early WIP. Auditing core is being stabilized. Telemetry SDK is experimental and may split into its own repo before v1.

---

## Table of Contents

- [About](#about)
- [Why](#why)
- [Status & Scope](#status--scope)
- [Install](#install)
- [Quickstart (minimal)](#quickstart-minimal)
- [Stores & Integrations](#stores--integrations)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [License](#license)

---

## About

**Mango.Auditing** aims to provide a focused, extensible auditing layer for .NET:
- **Capture** audit facts (who/what/when/where + metadata).
- **Persist** via pluggable stores (EF Core first).
- **Observe** via optional hooks (traces, metrics, logs).

This repository contains the solution and skeleton for code, tests, docs, and pipelines. (See folders: `/src`, `/tests`, `/docs`, `/build`, `/tools`.) :contentReference[oaicite:0]{index=0}

---

## Why

Most teams hand-roll ad-hoc audit tables and glue. That produces:
- inconsistent schemas,
- missing context (user, correlation, client info),
- zero observability around **auditing itself**.

Mango.Auditing standardizes the contract and gives you hooks for diagnostics without dictating your domain model.

---

## Status & Scope

- **Core auditing**: design settling (providers, writer/reader abstractions, EF store).
- **Telemetry SDK**: **experimental**; likely to be moved to a dedicated repo for clean SoC before v1.
- **Docs & samples**: incoming.

Current solution is MIT-licensed. :contentReference[oaicite:1]{index=1}

---

## Install

> Packages are not on NuGet yet. For now, use a project reference or local feed.

```bash
# (planned)
dotnet add package Mango.Auditing
dotnet add package Mango.Auditing.EntityFrameworkCore
dotnet add package Mango.Auditing.OpenTelemetry   # optional, may move to a separate repo
```

---

## Quickstart (minimal)

> **Note**: API is evolving. This shows the intended shape—not a final contract.

``` cs 
// 1) Register auditing (Core + EF store)
services.AddMangoAuditing(options =>
{
    options.Application("MyService");
    options.Environment("Prod");
    // options.EnrichWith<HttpContextEnricher>(); // example of custom enrichers
})
.AddEntityFrameworkStore<MyAppDbContext>(cfg =>
{
    cfg.Schema("audit");
    cfg.Table("AuditLog");
});

// 2) (Optional) Wire observability
services.AddMangoAuditingTelemetry(t =>
{
    t.EnableTracing();   // spans around write/read
    t.EnableMetrics();   // counters for audit writes, failures, latency
    t.EnableLogging();   // structured logs with correlationId
});

// 3) Use it in your code
public class OrdersController
{
    private readonly IAuditProvider _audit;

    public OrdersController(IAuditProvider audit) => _audit = audit;

    public async Task<IActionResult> Create(CreateOrder cmd, CancellationToken ct)
    {
        // ... your domain work
        await _audit.WriteAsync(AuditEntry.For("Order.Created")
            .Subject(cmd.CustomerId)
            .WithProperty("OrderId", order.Id)
            .WithProperty("Amount", order.Total)
            .WithActor(User.Identity?.Name)
            .Build(), ct);

        return Ok(order.Id);
    }
}
```

---

## Stores & Integrations

#### Planned/initial:
- EF Core store: simple schema, migrations, bulk insert support.
- File/Console store (dev/test).
- In-memory (unit tests).

#### Telemetry (optional):
- Tracing: Activity spans around audit writes/reads.
- Metrics: write counts, latency, error ratios.
- Logging: structured, correlation-aware.

> Telemetry bits may move to Mango.Telemetry (or similar) for strict separation of concerns.

---

## Roadmap
- [ ] Package IDs, namespaces, and branding finalized (Mango.*)
- [ ] Minimal EF Core store (schema + migrations + health checks)
- [ ] Writer/Reader abstractions (IAuditProvider, IAuditLogWriter, IAuditLogReader)
- [ ] Enrichers (ambient context: user, IP, correlationId, user agent)
- [ ] Telemetry hooks (tracing/metrics/logging) with toggles
- [ ] Samples: Web API + EF store; Console sample
- [ ] Docs: schema, DI setup, enrichment, performance notes
- [ ] NuGet publish (pre-release)

---

## Contributing
PRs welcome—keep scope tight and covered by tests. For API changes, open an issue first to discuss shape.
- Warnings as errors
- Nullable enabled
- Conventional commits preferred

---

## License
- MIT — see [LICENSE](LICENSE).
