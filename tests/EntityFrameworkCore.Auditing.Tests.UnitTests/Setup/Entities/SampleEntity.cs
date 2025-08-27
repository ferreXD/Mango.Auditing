// ReSharper disable once CheckNamespace

namespace EntityFrameworkCore.Auditing.Tests.UnitTests.Setup
{
    using System;

    public class SampleEntity : BaseEntity<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
