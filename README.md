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

## Executing the Tests

Run the application in GUI mode will display a UI similar to below with all of the test suites and test

![image-01](https://user-images.githubusercontent.com/117015/115127743-e75dbe00-9fa6-11eb-97dd-81b28bd7ef36.PNG)

Click on test case to run it. Once the test completes each test case will return the status of PASS, FAIL, or WARN

![image-02](https://user-images.githubusercontent.com/117015/115127815-99958580-9fa7-11eb-9385-6d457f51059e.PNG)

Click the **request** tab to view the request details
![image-03](https://user-images.githubusercontent.com/117015/115127830-be89f880-9fa7-11eb-841f-5af1356e89c7.PNG)

Click the **response** tab to iew the response details

![image-04](https://user-images.githubusercontent.com/117015/115127847-d3668c00-9fa7-11eb-9215-91c231ef3f94.PNG)



