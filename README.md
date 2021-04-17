# FastPass

An API test suite tool that support dependent API calls.

## Getting Started

### Create a console project

## Creating your first test

### Define the test suite

All test classes must be decorated with the attribute `[ApitTestSuite]`

```
using Gtt.FastPass.Attributes;
using Gtt.FastPass.Sample.Models;

namespace Gtt.FastPass.Sample.Tests
{
    [ApiTestSuite]
    public class DeckOfCardsTests
    {
    
    }
}
```

### Define a test
