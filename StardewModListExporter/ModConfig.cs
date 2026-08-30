namespace StardewModListExporter
{
    /// <summary>The mod's per-player settings, generated as config.json on first launch.</summary>
    public class ModConfig
    {
        /// <summary>Whether the mod list should be uploaded on game launch.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>The deployed Google Apps Script Web App URL (ends in /exec).</summary>
        public string WebAppUrl { get; set; } = "https://script.google.com/macros/s/REPLACE_WITH_YOUR_DEPLOYMENT_ID/exec";

        /// <summary>
        /// Optional shared secret sent with each request so the Apps Script can reject
        /// requests that didn't come from this mod. Must match the value checked in Code.gs.
        /// Leave blank to disable this check.
        /// </summary>
        public string SharedSecret { get; set; } = "";

        /// <summary>How long to wait for the upload to complete before giving up, in seconds.</summary>
        public int TimeoutSeconds { get; set; } = 15;
    }
}
