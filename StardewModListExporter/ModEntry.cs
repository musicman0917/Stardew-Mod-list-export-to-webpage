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

        /// <summary>Where to point this mod's own row in the exported sheet. SMAPI's UpdateKeys only
        /// recognizes specific sites (Nexus, GitHub, etc.), so a storefront link can't go there.</summary>
        private const string ProductPageUrl = "https://neighborhoodofmusic.com/products/sdv-export-mod-list-to-google-sheets";

        private ModConfig Config = null!;

        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        private async void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            this.SetUpGenericModConfigMenu();

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

        /// <summary>Register this mod's settings with Generic Mod Config Menu, if it's installed, so
        /// players can set WebAppUrl etc. from an in-game menu instead of hand-editing config.json.</summary>
        private void SetUpGenericModConfigMenu()
        {
            IGenericModConfigMenuApi? configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enabled",
                tooltip: () => "Whether the mod list should be uploaded on game launch.",
                getValue: () => this.Config.Enabled,
                setValue: value => this.Config.Enabled = value
            );

            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "Web App URL",
                tooltip: () => "The deployed Google Apps Script Web App URL (ends in /exec).",
                getValue: () => this.Config.WebAppUrl,
                setValue: value => this.Config.WebAppUrl = value
            );

            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "Shared Secret",
                tooltip: () => "Optional shared secret sent with each request, checked by the Apps Script. Leave blank to disable.",
                getValue: () => this.Config.SharedSecret,
                setValue: value => this.Config.SharedSecret = value
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Timeout (seconds)",
                tooltip: () => "How long to wait for the upload to complete before giving up.",
                getValue: () => this.Config.TimeoutSeconds,
                setValue: value => this.Config.TimeoutSeconds = value,
                min: 1,
                max: 120
            );
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
                    Version = mod.Manifest.Version.ToString(),
                    Url = mod.Manifest.UniqueID == this.ModManifest.UniqueID
                        ? ProductPageUrl
                        : ModPageUrlResolver.Resolve(mod.Manifest.UpdateKeys)
                })
                .ToList();
        }
    }
}
