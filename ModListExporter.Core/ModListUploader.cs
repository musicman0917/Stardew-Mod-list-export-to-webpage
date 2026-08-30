using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModListExporter.Core
{
    public static class ModListUploader
    {
        /// <summary>POST the payload to the given Web App URL. Never throws for HTTP-level failures; returns a result instead.</summary>
        public static async Task<ExportResult> ExportAsync(HttpClient client, string webAppUrl, ExportPayload payload, TimeSpan timeout)
        {
            try
            {
                string json = PayloadBuilder.ToJson(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var cts = new CancellationTokenSource(timeout);

                HttpResponseMessage response = await client.PostAsync(webAppUrl, content, cts.Token);

                return response.IsSuccessStatusCode
                    ? ExportResult.Ok((int)response.StatusCode)
                    : ExportResult.Failed((int)response.StatusCode, response.ReasonPhrase ?? "Request failed");
            }
            catch (OperationCanceledException)
            {
                return ExportResult.Failed(null, $"Request timed out after {timeout.TotalSeconds:0}s");
            }
            catch (Exception ex)
            {
                return ExportResult.Failed(null, ex.Message);
            }
        }
    }
}
