# smink
Smink is a test reporting tool (for now it converts xUnit test logs to pretty HTML)

## Running (for testing)

1. Clone the repository
1. In the root folder, run:

```bash
$ dotnet test ExampleTestProjects/xUnit.ExampleTests/xUnit.ExampleTests.sln --logger:"xunit;LogFilePath=/tmp/smink/{assembly}.testresults.xml"
$ dotnet run --project smink -- /tmp/logs/*.xml /tmp/logs/testreport.html
$ open /tmp/logs/testreport.html
```

And, view the beatiful HTML report, showing all run tests.

![image](https://github.com/erikbra/smink/assets/1628994/72555c58-92f6-4670-9b75-27cf01734833)


