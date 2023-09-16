namespace smink.Infrastructure;

public record ReportConfig
{
    public string[]  InputFiles { get; set; }
    public string  OutputFile { get; set; }
    public string? Title { get; set;  }
}