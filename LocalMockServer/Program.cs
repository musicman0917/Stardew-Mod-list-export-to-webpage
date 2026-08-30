using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using ModListExporter.Core;

// A tiny stand-in for the Google Apps Script Web App, so the SMAPI mod's HTTP
// posting logic can be exercised locally without deploying anything to Google.
// It mimics GoogleAppsScript/Code.gs: validates an optional shared secret,
// "clears the sheet," and rewrites a local text file with the same table shape.

string host = "http://localhost:8123/exec/";
string secret = "";
string outputPath = Path.Combine(AppContext.BaseDirectory, "mock-sheet.txt");

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--port" when i + 1 < args.Length:
            host = $"http://localhost:{args[++i]}/exec/";
            break;
        case "--secret" when i + 1 < args.Length:
            secret = args[++i];
            break;
        case "--out" when i + 1 < args.Length:
            outputPath = args[++i];
            break;
    }
}

using var listener = new HttpListener();
listener.Prefixes.Add(host);
listener.Start();

Console.WriteLine($"Local mock Apps Script listening at {host}");
Console.WriteLine($"Point the mod's config.json WebAppUrl at: {host.TrimEnd('/')}");
if (!string.IsNullOrEmpty(secret))
    Console.WriteLine("Shared secret check is ENABLED for this mock server.");
Console.WriteLine($"Mock sheet output: {outputPath}");
Console.WriteLine("Press Ctrl+C to stop.\n");

while (true)
{
    HttpListenerContext context = await listener.GetContextAsync();
    _ = HandleRequestAsync(context);
}

async System.Threading.Tasks.Task HandleRequestAsync(HttpListenerContext context)
{
    HttpListenerRequest request = context.Request;
    HttpListenerResponse response = context.Response;

    try
    {
        if (request.HttpMethod != "POST")
        {
            await WriteJsonAsync(response, 405, new { status = "error", message = "Only POST is supported." });
            return;
        }

        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        string body = await reader.ReadToEndAsync();

        ExportPayload? payload = JsonSerializer.Deserialize<ExportPayload>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (payload is null)
        {
            await WriteJsonAsync(response, 400, new { status = "error", message = "Invalid JSON body." });
            return;
        }

        if (!string.IsNullOrEmpty(secret) && payload.SharedSecret != secret)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Rejected request: bad shared secret.");
            await WriteJsonAsync(response, 403, new { status = "error", message = "Invalid shared secret." });
            return;
        }

        WriteMockSheet(payload);

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Wrote {payload.Mods.Count} mods to {outputPath}");
        await WriteJsonAsync(response, 200, new { status = "ok", modCount = payload.Mods.Count });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error handling request: {ex.Message}");
        await WriteJsonAsync(response, 500, new { status = "error", message = ex.Message });
    }
}

void WriteMockSheet(ExportPayload payload)
{
    var sb = new StringBuilder();
    sb.AppendLine($"Last updated: {payload.Timestamp}");
    sb.AppendLine();
    sb.AppendLine($"{"Mod Name",-30} {"Author",-20} {"Version",-10} ID");
    sb.AppendLine(new string('-', 90));

    foreach (ModRecord mod in payload.Mods.OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase))
        sb.AppendLine($"{mod.Name,-30} {mod.Author,-20} {mod.Version,-10} {mod.Id}");

    File.WriteAllText(outputPath, sb.ToString());
}

static async System.Threading.Tasks.Task WriteJsonAsync(HttpListenerResponse response, int statusCode, object body)
{
    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(body);
    response.StatusCode = statusCode;
    response.ContentType = "application/json";
    response.ContentLength64 = bytes.Length;
    await response.OutputStream.WriteAsync(bytes);
    response.OutputStream.Close();
}
