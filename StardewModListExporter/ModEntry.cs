using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using ModListExporter.Core;
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

            List<ModRecord> mods = this.GetLoadedMods();
            ExportPayload payload = PayloadBuilder.Build(mods, this.Config.SharedSecret);

            ExportResult result = await ModListUploader.ExportAsync(
                HttpClient,
                this.Config.WebAppUrl,
                payload,
                TimeSpan.FromSeconds(Math.Max(1, this.Config.TimeoutSeconds)));

            if (result.Success)
                this.Monitor.Log($"Exported {mods.Count} mods to Google Sheets.", LogLevel.Info);
            else
                this.Monitor.Log($"Mod list export failed (status {result.StatusCode}): {result.Error}", LogLevel.Error);
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
                .ToList();
        }
    }
}
