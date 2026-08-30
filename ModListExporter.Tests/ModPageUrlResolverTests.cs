using System.Collections.Generic;
using ModListExporter.Core;
using Xunit;

namespace ModListExporter.Tests
{
    public class ModPageUrlResolverTests
    {
        [Theory]
        [InlineData("Nexus:2400", "https://www.nexusmods.com/stardewvalley/mods/2400")]
        [InlineData("nexus:2400", "https://www.nexusmods.com/stardewvalley/mods/2400")]
        [InlineData("GitHub:Pathoschild/StardewMods", "https://github.com/Pathoschild/StardewMods")]
        [InlineData("Chucklefish:4250", "https://community.playstarbound.com/resources/4250")]
        [InlineData("ModDrop:12345", "https://www.moddrop.com/stardew-valley/mods/12345")]
        [InlineData("CurseForge:99", "https://www.curseforge.com/stardewvalley/mods/99")]
        public void Resolve_MapsKnownRepositoryToUrl(string updateKey, string expectedUrl)
        {
            string result = ModPageUrlResolver.Resolve(new[] { updateKey });

            Assert.Equal(expectedUrl, result);
        }

        [Fact]
        public void Resolve_ReturnsFirstRecognizedKey_WhenMultipleGiven()
        {
            string result = ModPageUrlResolver.Resolve(new[] { "SomethingUnknown:1", "Nexus:42" });

            Assert.Equal("https://www.nexusmods.com/stardewvalley/mods/42", result);
        }

        [Fact]
        public void Resolve_ReturnsEmpty_WhenNoKeysRecognized()
        {
            string result = ModPageUrlResolver.Resolve(new[] { "SomethingUnknown:1", "AlsoUnknown:2" });

            Assert.Equal("", result);
        }

        [Fact]
        public void Resolve_ReturnsEmpty_WhenNoUpdateKeys()
        {
            Assert.Equal("", ModPageUrlResolver.Resolve(null));
            Assert.Equal("", ModPageUrlResolver.Resolve(new List<string>()));
        }
    }
}
