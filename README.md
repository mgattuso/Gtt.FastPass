# FastPass

An API test suite tool that support dependent API calls.

## Getting Started

### Create a console project

1. Create a dotnet core console application
2. Add a reference to Gtt.FastPass nuget package
3. Create an instance of the FastPassEndpoint class with a URL reference to the API endpoint.
4. Use the Visual Studio tools to start multiple projects, the API and the Console Project.

### Basic console app that runs the Test GUI

```
class Program
    {
        static int Main(string[] args)
        {
            var root = new FastPassEndpoint("http://deckofcardsapi.com");
            FastPassTestRunner<TestModel>.RunAsGui(root);
            return 0;
        }
    }
```


## Creating your first test

### Define the test suite

All test classes must be decorated with the attribute `[ApitTestSuite]`

```
using Gtt.FastPass;

namespace Gtt.FastPass.Sample.Tests
{
    [ApiTestSuite]
    public class DeckOfCardsTests
    {
    
    }
}
```

### Define a test

All tests must be decorated with the `[ApiTest]` attribute and must accept a `FastPassEndpoint` object parameter:

```
[ApiTest]
public void ShuffleDeck(FastPassEndpoint test)
{
    test
        .Endpoint("api") 
        .WithHeader("Content-Type", "application/json")
        .WithHeader("Accepts", "application/json")
        .Get("deck/new/shuffle/?deck_count=1")
        .AssertStatusCode(200)
        .AssertMaxResponseTimeMs(1000)
        .AssertBody<DeckResponse>("Contains deck_id", x => !string.IsNullOrWhiteSpace(x.Deck_id))
        .WriteResults();
}
```

### Basic test configuration

Each test must have the following options:

```
.Endpoint()
.Endpoint(<path>)
```
Each test must call the .EndPoint method. An optional path can be provided to scope the test to a specific endpoint.

The following optional request options can be set

```
.WithHeader(<name>, <value>)
```
Provide a headers values
