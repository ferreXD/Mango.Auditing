// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Security;
    using System.Text.Json;

    public static class EntityEntryExtensions
    {
        public static string GetEntityId(this EntityEntry entry)
        {
            var keyProps = entry.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToList();

            switch (keyProps.Count)
            {
                case 0:
                    throw new InvalidOperationException("No primary key defined for entity.");
                case > 1:
                    throw new InvalidOperationException("Composite primary key detected. Use GetPrimaryKey() instead.");
            }

            var value = keyProps[0].CurrentValue;
            return value is not null
                ? value.ToString()!
                : throw new InvalidOperationException("Primary key value is null. Entity may not be persisted yet.");
        }

        public static string GetPrimaryKey(this EntityEntry entry)
        {
            var keyProps = entry.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToList();

            if (keyProps.Count == 0)
                throw new InvalidOperationException("No primary key defined for entity.");

            var segments = keyProps.Select(p =>
            {
                var name = p.Metadata.Name;
                var value = p.CurrentValue ?? "null";

                // Escape commas in string values
                return value is string str && str.Contains(',')
                    ? $"{name}=\"{str}\""
                    : $"{name}={value}";
            });

            return string.Join(",", segments);
        }

        public static string GetAffectedColumns(this EntityEntry entry)
        {
            return string.Join(",", entry.Properties
                .Where(p => p.IsModified)
                .Select(p => p.Metadata.Name));
        }

        public static string? GetOldValues(this EntityEntry entry, ISensitiveDataFilter sensitiveDataFilter, bool includeEntityValues)
        {
            if (!includeEntityValues) return null;

            var values = entry.Properties
                .Where(p => p.IsModified)
                .ToDictionary(
                    p => p.Metadata.Name,
                    p => sensitiveDataFilter.IsSensitive(entry.Entity.GetType().Name, p.Metadata.Name) ? sensitiveDataFilter.MaskValue(p.OriginalValue) : p.OriginalValue
                );

            return JsonSerializer.Serialize(values);
        }

        public static string? GetNewValues(this EntityEntry entry, ISensitiveDataFilter sensitiveDataFilter, bool includeEntityValues)
        {
            if (!includeEntityValues) return null;

            var values = entry.Properties
                .Where(p => p.IsModified)
                .ToDictionary(
                    p => p.Metadata.Name,
                    p => sensitiveDataFilter.IsSensitive(entry.Entity.GetType().Name, p.Metadata.Name) ? sensitiveDataFilter.MaskValue(p.CurrentValue) : p.CurrentValue
                );

            return JsonSerializer.Serialize(values);
        }
    }
}