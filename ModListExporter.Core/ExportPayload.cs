using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ModListExporter.Core
{
    /// <summary>The JSON body posted to the Google Apps Script Web App.</summary>
    public class ExportPayload
    {
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = "";

        [JsonPropertyName("sharedSecret")]
        public string SharedSecret { get; set; } = "";

        [JsonPropertyName("mods")]
        public List<ModRecord> Mods { get; set; } = new();
    }
}
