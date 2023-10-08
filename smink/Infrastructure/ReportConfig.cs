namespace smink.Infrastructure;

public record ReportConfig
{
    public string[] InputFiles { get; set; } = Array.Empty<string>();
    public string OutputFile { get; set; } = null!;
    public string? Title { get; set;  }
}