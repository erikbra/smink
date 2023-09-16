namespace smink.Models.NUnit;

public record Failure
{
    public string? Message { get; set; }
    public string? StackTrace { get; set; }
}