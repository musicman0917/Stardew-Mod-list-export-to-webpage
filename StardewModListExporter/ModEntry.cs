using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace StardewModListExporter
{
    public class ModEntry : Mod
    {
        /// <summary>Shared HTTP client for the mod's lifetime (HttpClient is meant to be reused, not disposed per call).</summary>
        private static readonly HttpClient HttpClient = new();

        private ModConfig Config = null!;

        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        private async void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            if (!this.Config.Enabled)
                return;

            if (string.IsNullOrWhiteSpace(this.Config.WebAppUrl) || this.Config.WebAppUrl.Contains("REPLACE_WITH_YOUR_DEPLOYMENT_ID"))
            {
                this.Monitor.Log("Mod list export skipped: set WebAppUrl in config.json to your deployed Google Apps Script URL.", LogLevel.Warn);
                return;
            }

            try
            {
                List<ModRecord> mods = this.GetLoadedMods();
                await this.ExportModListAsync(mods);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Failed to export mod list: {ex}", LogLevel.Error);
            }
        }

        /// <summary>Collect the id, name, author, and version of every currently loaded mod.</summary>
        private List<ModRecord> GetLoadedMods()
        {
            return this.Helper.ModRegistry.GetAll()
                .Select(mod => new ModRecord
                {
                    Id = mod.Manifest.UniqueID,
                    Name = mod.Manifest.Name,
                    Author = mod.Manifest.Author,
                    Version = mod.Manifest.Version.ToString()
                })
                .OrderBy(mod => mod.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>POST the mod list to the configured Google Apps Script Web App.</summary>
        private async Task ExportModListAsync(List<ModRecord> mods)
        {
            var payload = new ExportPayload
            {
                Timestamp = DateTimeOffset.UtcNow.ToString("o"),
                SharedSecret = this.Config.SharedSecret,
                Mods = mods
            };

            string json = JsonSerializer.Serialize(payload);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(1, this.Config.TimeoutSeconds)));

            HttpResponseMessage response = await HttpClient.PostAsync(this.Config.WebAppUrl, content, cts.Token);

            if (response.IsSuccessStatusCode)
                this.Monitor.Log($"Exported {mods.Count} mods to Google Sheets.", LogLevel.Info);
            else
                this.Monitor.Log($"Mod list export failed with status {(int)response.StatusCode} {response.ReasonPhrase}.", LogLevel.Error);
        }

        private class ModRecord
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = "";

            [JsonPropertyName("name")]
            public string Name { get; set; } = "";

            [JsonPropertyName("author")]
            public string Author { get; set; } = "";

            [JsonPropertyName("version")]
            public string Version { get; set; } = "";
        }

        private class ExportPayload
        {
            [JsonPropertyName("timestamp")]
            public string Timestamp { get; set; } = "";

            [JsonPropertyName("sharedSecret")]
            public string SharedSecret { get; set; } = "";

            [JsonPropertyName("mods")]
            public List<ModRecord> Mods { get; set; } = new();
        }
    }
}
