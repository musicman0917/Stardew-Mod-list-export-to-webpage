using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ModListExporter.Core;
using Xunit;

namespace ModListExporter.Tests
{
    public class ModListUploaderTests
    {
        private class FakeHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _status;
            public string? LastRequestBody;
            public Uri? LastRequestUri;

            public FakeHandler(HttpStatusCode status) => _status = status;

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequestUri = request.RequestUri;
                LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
                return new HttpResponseMessage(_status);
            }
        }

        private class ThrowingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => throw new HttpRequestException("connection refused");
        }

        [Fact]
        public async Task ExportAsync_ReturnsSuccess_OnHttp200()
        {
            var handler = new FakeHandler(HttpStatusCode.OK);
            using var client = new HttpClient(handler);
            var payload = PayloadBuilder.Build(new List<ModRecord>(), sharedSecret: "secret", DateTimeOffset.UnixEpoch);

            ExportResult result = await ModListUploader.ExportAsync(client, "http://localhost:9999/exec", payload, TimeSpan.FromSeconds(5));

            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("http://localhost:9999/exec", handler.LastRequestUri!.ToString());
            Assert.Contains("\"sharedSecret\":\"secret\"", handler.LastRequestBody);
        }

        [Fact]
        public async Task ExportAsync_ReturnsFailure_OnNon2xxStatus()
        {
            var handler = new FakeHandler(HttpStatusCode.InternalServerError);
            using var client = new HttpClient(handler);
            var payload = PayloadBuilder.Build(new List<ModRecord>(), sharedSecret: "");

            ExportResult result = await ModListUploader.ExportAsync(client, "http://localhost:9999/exec", payload, TimeSpan.FromSeconds(5));

            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task ExportAsync_ReturnsFailure_WithoutThrowing_WhenRequestFails()
        {
            using var client = new HttpClient(new ThrowingHandler());
            var payload = PayloadBuilder.Build(new List<ModRecord>(), sharedSecret: "");

            ExportResult result = await ModListUploader.ExportAsync(client, "http://localhost:9999/exec", payload, TimeSpan.FromSeconds(5));

            Assert.False(result.Success);
            Assert.Null(result.StatusCode);
            Assert.NotNull(result.Error);
        }
    }
}
