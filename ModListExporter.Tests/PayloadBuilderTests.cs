using System;
using System.Collections.Generic;
using ModListExporter.Core;
using Xunit;

namespace ModListExporter.Tests
{
    public class PayloadBuilderTests
    {
        [Fact]
        public void Build_SortsModsByNameCaseInsensitively()
        {
            var mods = new List<ModRecord>
            {
                new() { Id = "b.mod", Name = "zeta mod", Author = "B", Version = "1.0.0" },
                new() { Id = "a.mod", Name = "Alpha Mod", Author = "A", Version = "2.0.0" },
                new() { Id = "c.mod", Name = "beta mod", Author = "C", Version = "3.0.0" },
            };

            ExportPayload payload = PayloadBuilder.Build(mods, sharedSecret: "");

            Assert.Equal(new[] { "Alpha Mod", "beta mod", "zeta mod" }, payload.Mods.ConvertAll(m => m.Name));
        }

        [Fact]
        public void Build_SetsTimestampAndSharedSecret()
        {
            var timestamp = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);

            ExportPayload payload = PayloadBuilder.Build(new List<ModRecord>(), sharedSecret: "topsecret", timestamp);

            Assert.Equal("2026-01-02T03:04:05.0000000+00:00", payload.Timestamp);
            Assert.Equal("topsecret", payload.SharedSecret);
        }

        [Fact]
        public void ToJson_ProducesExpectedPropertyNames()
        {
            var payload = PayloadBuilder.Build(
                new List<ModRecord> { new() { Id = "x.y", Name = "X", Author = "Y", Version = "1.2.3" } },
                sharedSecret: "s",
                timestamp: DateTimeOffset.UnixEpoch);

            string json = PayloadBuilder.ToJson(payload);

            Assert.Contains("\"timestamp\":", json);
            Assert.Contains("\"sharedSecret\":\"s\"", json);
            Assert.Contains("\"id\":\"x.y\"", json);
            Assert.Contains("\"name\":\"X\"", json);
            Assert.Contains("\"author\":\"Y\"", json);
            Assert.Contains("\"version\":\"1.2.3\"", json);
        }
    }
}
