namespace smink.Models.Report;

public record TestFailure
{
    public string? ExceptionType { get; set; }
    public string? Message { get; set; }
    public string? StackTrace { get; set; }
}