using System.Text.Encodings.Web;

namespace smink.Models.Report;

public class TestRun
{
    public Guid Id { get; set;  }
    public string? Type { get; set; }
    public string? Method { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string SanitizedName => Common.Sanitize(Name ?? Id.ToString());
    
    public string? Result { get; set; }
    public string? SourceFile { get; set; }
    public string? SourceLine { get; set; }
    public decimal Time { get; set; }
    
    public TestFailure? Failure { get; set; }
    public string? Output { get; set; }
    public string? Reason { get; set; }
    

    public bool HasContent => Failure is {} || Reason is {} || Output is {};
}