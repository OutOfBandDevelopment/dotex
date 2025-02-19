**README**

**Summary**
The provided source files describe a set of extensions for the .NET Core Service Collection library. These extensions enable the addition of various features to an application, including support for shared search query extensions, query builders, and expression tree builders. The provided static class `ServiceCollectionExtensions` extends the `IServiceCollection` interface to provide these features.

**Technical Summary**
The design pattern used in these files is the Singleton Pattern, which is implemented through the use of static classes and methods. This pattern ensures that only one instance of each service is created and provides a global point of access to these services.

In terms of architectural patterns, the files follow the Service Provider pattern, which is a common pattern in .NET Core applications. This pattern involves using the ServiceCollection to manage the lifetime and configuration of services in an application.

**Component Diagram (PlantUML)**

```plantuml
@startuml
class ServiceCollectionExtensions {
  - extension Method: TryAddSearchQueryExtensions
  - extension Method: TryAddTransient
  - extension Method: TryAddScoped
}

class QueryBuilder {
  - implements IQueryBuilder<T>
}

class SortBuilder {
  - implements ISortBuilder<T>
}

class ExpressionTreeBuilder {
  - implements IExpressionTreeBuilder<T>
}

class PostBuildExpressionVisitor {
  - implements IPostBuildExpressionVisitor
}

class CaptureResultMessage {
  - implements ICaptureResultMessage
}

ServiceCollectionExtensions -->> QueryBuilder
ServiceCollectionExtensions -->> SortBuilder
ServiceCollectionExtensions -->> ExpressionTreeBuilder
ServiceCollectionExtensions -->> PostBuildExpressionVisitor
ServiceCollectionExtensions -->> CaptureResultMessage
@enduml
```
This diagram illustrates the relationships between the ServiceCollectionExtensions class and the various services it provides, including QueryBuilder, SortBuilder, ExpressionTreeBuilder, PostBuildExpressionVisitor, and CaptureResultMessage. The diagram shows that each of these services is added to the ServiceCollection using the TryAddTransient and TryAddScoped methods.