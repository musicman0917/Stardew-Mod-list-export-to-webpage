using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ModListExporter.Core
{
    public static class PayloadBuilder
    {
        /// <summary>Build the export payload from a raw mod list, sorted by name for a stable, readable sheet.</summary>
        public static ExportPayload Build(IEnumerable<ModRecord> mods, string sharedSecret, DateTimeOffset? timestamp = null)
        {
            return new ExportPayload
            {
                Timestamp = (timestamp ?? DateTimeOffset.UtcNow).ToString("o"),
                SharedSecret = sharedSecret ?? "",
                Mods = mods
                    .OrderBy(mod => mod.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };
        }

        public static string ToJson(ExportPayload payload) => JsonSerializer.Serialize(payload);
    }
}
