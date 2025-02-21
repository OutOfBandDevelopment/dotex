Here is the documentation for the provided source code files, including class diagrams in Plant UML.

**TikaConversionHandlerBase.cs**

This is an abstract class that provides a base implementation for document conversion handlers using Apache Tika. It defines the interface for converting documents from a source format to a destination format.

```plantuml
@startuml
class TikaConversionHandlerBase {
  +Destinations: string[]
  +Sources: string[]
  +SupportedDestination(string): bool
  +SupportedSource(string): bool
  ConvertAsync(Stream, string, Stream, string): Task
}

interface IDocumentConversionHandler {
  +ConvertAsync(Stream, string, Stream, string): Task
}

TikaConversionHandlerBase --|> IDocumentConversionHandler
@enduml
```

**TikaToHtmlConversionBaseHandler.cs**

This is an abstract class that provides a base implementation for handlers that convert documents to HTML using Apache Tika. It inherits from `TikaConversionHandlerBase` and provides a default implementation for converting documents to HTML.

```plantuml
@startuml
class TikaToHtmlConversionBaseHandler {
  +Destinations: string[]
  +ConvertAsync(Stream, string, Stream, string): Task
}

class TikaConversionHandlerBase {
  +ConvertAsync(Stream, string, Stream, string): Task
}

TikaToHtmlConversionBaseHandler --|> TikaConversionHandlerBase
@enduml
```

**TikaDocToHtmlConversionHandler.cs**

This is a concrete class that provides a handler for converting Microsoft Word documents to HTML using Apache Tika. It inherits from `TikaToHtmlConversionBaseHandler` and provides a specific implementation for converting Microsoft Word documents to HTML.

```plantuml
@startuml
class TikaDocToHtmlConversionHandler {
  +Sources: string[]
  +ConvertAsync(Stream, string, Stream, string): Task
}

class TikaToHtmlConversionBaseHandler {
  +ConvertAsync(Stream, string, Stream, string): Task
}

TikaDocToHtmlConversionHandler --|> TikaToHtmlConversionBaseHandler
@enduml
```

**TikaDocxToHtmlConversionHandler.cs**

This is a concrete class that provides a handler for converting Microsoft Word (DOCX) documents to HTML using Apache Tika. It inherits from `TikaToHtmlConversionBaseHandler` and provides a specific implementation for converting Microsoft Word (DOCX) documents to HTML.

```plantuml
@startuml
class TikaDocxToHtmlConversionHandler {
  +Sources: string[]
  +ConvertAsync(Stream, string, Stream, string): Task
}

class TikaToHtmlConversionBaseHandler {
  +ConvertAsync(Stream, string, Stream, string): Task
}

TikaDocxToHtmlConversionHandler --|> TikaToHtmlConversionBaseHandler
@enduml
```

**TikaEpubToHtmlConversionHandler.cs**

This is a concrete class that provides a handler for converting EPUB files to HTML using Apache Tika. It inherits from `TikaToHtmlConversionBaseHandler` and provides a specific implementation for converting EPUB files to HTML.

```plantuml
@startuml
class TikaEpubToHtmlConversionHandler {
  +Sources: string[]
  +ConvertAsync(Stream, string, Stream, string): Task
}

class TikaToHtmlConversionBaseHandler {
  +ConvertAsync(Stream, string, Stream, string): Task
}

TikaEpubToHtmlConversionHandler --|> TikaToHtmlConversionBaseHandler
@enduml
```

**TikaOdtToHtmlConversionHandler.cs**

This is a concrete class that provides a handler for converting OpenDocument Text (ODT) documents to HTML using Apache Tika. It inherits from `TikaToHtmlConversionBaseHandler` and provides a specific implementation for converting OpenDocument Text (ODT) documents to HTML.

```plantuml
@startuml
class TikaOdtToHtmlConversionHandler {
  +Sources: string[]
  +ConvertAsync(Stream, string, Stream, string): Task
}

class TikaToHtmlConversionBaseHandler {
  +ConvertAsync(Stream, string, Stream, string): Task
}

TikaOdtToHtmlConversionHandler --|> TikaToHtmlConversionBaseHandler
@enduml
```

**TikaPdfToHtmlConversionHandler.cs**

This is a concrete class that provides a handler for converting PDF documents to HTML using Apache Tika. It inherits from `TikaToHtmlConversionBaseHandler` and provides a specific implementation for converting PDF documents to HTML.

```plantuml
@startuml
class TikaPdfToHtmlConversionHandler {
  +Sources: string[]
  +ConvertAsync(Stream, string, Stream, string): Task
}

class TikaToHtmlConversionBaseHandler {
  +ConvertAsync(Stream, string, Stream, string): Task
}

TikaPdfToHtmlConversionHandler --|> TikaToHtmlConversionBaseHandler
@enduml
```

