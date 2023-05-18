using System.Globalization;
using System.Xml.Linq;
using smink.Models.XUnit;

namespace smink.TestResultReaders;

// https://xunit.net/docs/format-xml-v2

public class XUnitResultsReader
{
    public async Task<Assemblies?> Load(params string[] files)
    {
        var ct = new CancellationToken();
        
        // Just use the first file as the "Assemblies" node - there's not much interesting
        // info here anyway :)
        var firstFile = files.FirstOrDefault();
        Assemblies? assemblies = new Assemblies();
        
        if (firstFile is { })
        {
            await using var stream = File.OpenRead(firstFile);
            var doc = await XDocument.LoadAsync(stream, LoadOptions.PreserveWhitespace, ct);
            assemblies = Parse(doc);
        }
        
        foreach (var file in files.Skip(1))
        {
            await using var stream = File.OpenRead(file);
            var doc = await XDocument.LoadAsync(stream, LoadOptions.PreserveWhitespace, ct);
            var theseAssemblies = Parse(doc);
            if (theseAssemblies is { })
            {
                assemblies?.AssembliesList.AddRange(theseAssemblies.AssembliesList);
            }
        }
        
        return assemblies;
    }

    private Assemblies? Parse(XDocument doc)
    {
        return doc.Root switch
        {
            { } root => new Assemblies()
                {
                    Id = GetAttribute<Guid>(root, "id"),
                    SchemaVersion = GetAttribute<int>(root, "schema-version"),
                    Computer = GetAttribute(root, "computer"),
                    User = GetAttribute(root, "user"),
                    Timestamp = GetAttribute(root, "timestamp"),
                    StartRtf = GetAttribute<DateTime>(root, "start-rtf"),
                    FinishRtf = GetAttribute<DateTime>(root, "finish-rtf"),
                    AssembliesList = GetAssemblies(root.Elements(XName.Get("assembly"))),
                },
            _ => null
        };
    }


    private List<Assembly> GetAssemblies(IEnumerable<XElement> elements)
    {
        return elements.Select(element => new Assembly()
            {
                Id = GetAttribute<Guid>(element, "id"),
                Name = GetAttribute(element, "name"),
                ConfigFile = GetAttribute(element, "config-file"),
                Environment = GetAttribute(element, "environment"),
                StartRtf = GetAttribute<DateTime>(element, "start-rtf"),
                FinishRtf = GetAttribute<DateTime>(element, "finish-rtf"),
                TimeRtf = GetAttribute<TimeSpan>(element, "time-rtf"),
                RunDate = GetAttribute(element, "run-date"),
                RunTime = GetAttribute(element, "run-time"),
                Time = GetAttribute<decimal>(element, "time"),
                TargetFramework = GetAttribute(element, "target-framework"),
                TestFramework = GetAttribute(element, "test-framework"),
                
                Passed = GetAttribute<int>(element, "passed"),
                NotRun = GetAttribute<int>(element, "not-run"),
                Skipped = GetAttribute<int>(element, "skipped"),
                Errors = GetAttribute<int>(element, "errors"),
                Failed = GetAttribute<int>(element, "failed"),
                Total = GetAttribute<int>(element, "total"),
                
                Collections = GetCollections(element.Elements(XName.Get("collection"))),
                ErrorsList = GetErrors(element.Element(XName.Get("errors")))
            })
            .ToList();
    }

    private List<Error> GetErrors(XElement? elements)
    {
        return elements switch
        {
            { } => elements.Elements("error")
                .Select(element => new Error()
                {
                    Name = GetAttribute(element, "name"),
                    Type = GetEnumAttribute<ErrorType>(element, "type"),
                    Failure = GetFailure(element.Element("failure"))
                })
                .ToList(),
            _ => Enumerable.Empty<Error>().ToList(),
        };
    }

    private List<Collection> GetCollections(IEnumerable<XElement> elements)
    {
        return elements.Select(element => new Collection()
        {
                Id = GetAttribute<Guid>(element, "id"),
                Name = GetAttribute(element, "name"),
                
                Passed = GetAttribute<int>(element, "passed"),
                NotRun = GetAttribute<int>(element, "not-run"),
                Skipped = GetAttribute<int>(element, "skipped"),
                Errors = GetAttribute<int>(element, "errors"),
                Failed = GetAttribute<int>(element, "failed"),
                Total = GetAttribute<int>(element, "total"),
                
                Time = GetAttribute<decimal>(element, "time"),
                TimeRtf = GetAttribute<TimeSpan>(element, "time-rtf"),
                
                Tests = GetTests(element.Elements(XName.Get("test")))

        })
        .ToList();
    }

    private List<Test> GetTests(IEnumerable<XElement> elements)
    {
        return elements.Select(element => new Test()
            {
                Id = GetAttribute<Guid>(element, "id"),
                Name = GetAttribute(element, "name"),
                
                Type = GetAttribute(element, "type"),
                Method = GetAttribute(element, "method"),
                
                Result = GetEnumAttribute<Result>(element, "result"),
                SourceFile = GetAttribute(element, "source-file"),
                SourceLine = GetAttribute(element, "source-line"),
                
                Time = GetAttribute<decimal>(element, "time"),
                TimeRtf = GetAttribute<TimeSpan>(element, "time-rtf"),
                
                Failure = GetFailure(element.Element("failure")),
                Output = element.Element("output")?.Value,
                Reason = element.Element("reason")?.Value,
                
                Traits = GetTraits(element.Elements("traits"))

            })
            .ToList();
    }

    private List<Trait> GetTraits(IEnumerable<XElement> elements)
    {
        return elements.Select(element => new Trait()
        {
            Name = GetAttribute(element, "name"),
            Value = GetAttribute(element, "value")
        })
        .ToList();
    }

    private Failure? GetFailure(XElement? element)
    {
        return element switch
        {
            { } => new Failure()
            {
                ExceptionType = GetAttribute(element, "exception-type"),
                Message = element.Element("message")?.Value,
                StackTrace = element.Element("stack-trace")?.Value
            },
            _ => null
        };
    }

    private static string? GetAttribute(XElement? elem, string name)
    {
        return elem?.Attribute(XName.Get(name))?.Value;
    }
    
    private static T? GetAttribute<T>(XElement? elem, string name) where T: IParsable<T>
    {
        return GetAttribute(elem, name) switch
        {
            { } val => T.Parse(val, CultureInfo.InvariantCulture),
            _ => default
        };
    }
    
    private static T? GetEnumAttribute<T>(XElement? elem, string name) where T: struct, Enum
    {
        return GetAttribute(elem, name) switch
        {
            { } val => Enum.Parse<T>(val),
            _ => default
        };
    }

}