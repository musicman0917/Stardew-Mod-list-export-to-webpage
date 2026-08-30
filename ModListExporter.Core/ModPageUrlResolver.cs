using System.Collections.Generic;

namespace ModListExporter.Core
{
    /// <summary>
    /// Turns a mod's SMAPI update keys (e.g. "Nexus:2400", "GitHub:Pathoschild/StardewMods")
    /// into a link to that mod's page, so the exported sheet can tell you where to find it.
    /// </summary>
    public static class ModPageUrlResolver
    {
        /// <summary>Resolve the first update key that maps to a known mod site, or "" if none do.</summary>
        public static string Resolve(IEnumerable<string>? updateKeys)
        {
            if (updateKeys == null)
                return "";

            foreach (string rawKey in updateKeys)
            {
                string url = TryResolveOne(rawKey);
                if (url.Length > 0)
                    return url;
            }

            return "";
        }

        private static string TryResolveOne(string rawKey)
        {
            if (string.IsNullOrWhiteSpace(rawKey))
                return "";

            int separatorIndex = rawKey.IndexOf(':');
            if (separatorIndex < 0)
                return "";

            string repository = rawKey.Substring(0, separatorIndex).Trim();
            string id = rawKey.Substring(separatorIndex + 1).Trim();
            if (id.Length == 0)
                return "";

            return repository.ToLowerInvariant() switch
            {
                "nexus" => $"https://www.nexusmods.com/stardewvalley/mods/{id}",
                "github" => $"https://github.com/{id}",
                "chucklefish" => $"https://community.playstarbound.com/resources/{id}",
                "moddrop" => $"https://www.moddrop.com/stardew-valley/mods/{id}",
                "curseforge" => $"https://www.curseforge.com/stardewvalley/mods/{id}",
                _ => ""
            };
        }
    }
}
