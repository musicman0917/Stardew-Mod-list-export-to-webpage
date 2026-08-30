namespace ModListExporter.Core
{
    public class ExportResult
    {
        public bool Success { get; init; }
        public int? StatusCode { get; init; }
        public string? Error { get; init; }

        public static ExportResult Ok(int statusCode) => new() { Success = true, StatusCode = statusCode };
        public static ExportResult Failed(int? statusCode, string error) => new() { Success = false, StatusCode = statusCode, Error = error };
    }
}
