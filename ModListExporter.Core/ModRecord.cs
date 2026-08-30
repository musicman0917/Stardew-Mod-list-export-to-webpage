using System.Text.Json.Serialization;

namespace ModListExporter.Core
{
    /// <summary>A single mod's identity, independent of any SMAPI type so it can be unit tested without the game installed.</summary>
    public class ModRecord
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("author")]
        public string Author { get; set; } = "";

        [JsonPropertyName("version")]
        public string Version { get; set; } = "";

        [JsonPropertyName("url")]
        public string Url { get; set; } = "";
    }
}
