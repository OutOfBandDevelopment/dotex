# StreamExtensionsTests

## Overview

This test class is used to test the `OoBDev.Extensions.IO` namespace, specifically the `StreamExtensions` class. The class provides extensions methods for working with streams in .NET.

## Tests

### CopyOfTest

This test method checks whether the `CopyOf` method correctly creates a new stream that is a copy of the original stream.

```plantuml
@startuml
class StreamExtensionsTests {
  - TestContext: TestContext
  + CopyOfTest()
}

@TestContext extends TestContext

@startstate TestContext
@endstate

start
Activating StreamExtensionsTests
 activates TestContext
while StreamExtensionsTests.CopyOfTest
```plantuml

```md
### Sequence Diagram

participant StreamExtensionsTests
participant TestContext
participant StreamExtensions
participant MemoryStream

note right of StreamExtensionsTests: StreamExtensionsTests class
note right of TestContext: TestContext instance
note left of StreamExtensions: StreamExtensions class
note left of MemoryStream: MemoryStream instance

StreamExtensionsTests ->> TestContext: GetTestContext
 activate TestContext
TestContext ->> StreamExtensions: CopyOf
 activate StreamExtensions
StreamExtensions ->> MemoryStream: Copy
 activate MemoryStream
MemoryStream ->> StreamExtensions: Length
 activate StreamExtensions
StreamExtensions ->> TestExtensions: AreEqual(ms.Length, ms2.Length)
 activate TestExtensions
TestExtensions ->> StreamExtensions: AreNotEqual(ms, ms2)
 activate StreamExtensions

StreamExtensionsTests ->> test outcome: passing or failing
 deactivate TestExtensions
 deactivate MemoryStream
 deactivate StreamExtensions
 deactivate TestContext
 deactivate StreamExtensionsTests
```