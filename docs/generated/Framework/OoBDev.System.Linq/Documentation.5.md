Here is the documentation for the source code in markdown format, including a class diagram, component model, and sequence diagram in Plant UML:

# ServiceCollectionExtensions

## Overview

`ServiceCollectionExtensions` is a static class that provides a set of extension methods for the Microsoft.Extensions.DependencyInjection `IServiceCollection` interface. These methods aim to suggest IOC (Inversion of Control) configurations for various shared services and components.

## Class Diagram

```plantuml
@startuml
class ServiceCollectionExtensions {
  - TryAddSearchQueryExtensions(services: IServiceCollection): IServiceCollection
  - TryAddTransient<T>(services: IServiceCollection): IServiceCollection
  - TryAddScoped<T>(services: IServiceCollection): IServiceCollection
  - TryAddTransient<T1, T2>(services: IServiceCollection): IServiceCollection
}

class IServiceCollection {
  + AddTransient<T>(services: IServiceCollection): IServiceCollection
  + AddScoped<T>(services: IServiceCollection): IServiceCollection
}

class QueryBuilder<T> {
  + ... (methods and properties)
}

class SortBuilder<T> {
  + ... (methods and properties)
}

class ExpressionTreeBuilder<T> {
  + ... (methods and properties)
}

class PostBuildExpressionVisitor {
  + ... (methods and properties)
}

class CaptureResultMessage implements ICaptureResultMessage {
  + ... (methods and properties)
}

ICaptureResultMessage -- interface
PostBuildExpressionVisitor -- implements ICaptureResultMessage
ServiceCollectionExtensions --+ TryAddSearchQueryExtensions
ServiceCollectionExtensions --+ TryAddTransient
ServiceCollectionExtensions --+ TryAddScoped
QueryBuilder --+ implements IQueryBuilder
SortBuilder --+ implements ISortBuilder
ExpressionTreeBuilder --+ implements IExpressionTreeBuilder
@enduml
```

## Component Model

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-remote/plantuml-remote/master/src/main/svg/UML.svg

component "ServiceLocator" as service_locator {
  component "ServiceCollection" {
    interface IServiceCollection
  }
  component "Search Query Extensions" {
    class QueryBuilder<T>
    class SortBuilder<T>
    class ExpressionTreeBuilder<T>
    interface IQueryBuilder<T>
    interface ISortBuilder<T>
    interface IExpressionTreeBuilder<T>
  }
  component "Expression Visitors" {
    class PostBuildExpressionVisitor
  }
  component "Message" {
    interface ICaptureResultMessage
    class CaptureResultMessage
  }
}

service_locator ->> ServiceCollection : TryAddSearchQueryExtensions
ServiceCollection ->> QueryBuilder<T> : TryAddTransient
ServiceCollection ->> SortBuilder<T> : TryAddTransient
ServiceCollection ->> ExpressionTreeBuilder<T> : TryAddTransient
ServiceCollection ->> PostBuildExpressionVisitor : AddTransient
ServiceCollection ->> CaptureResultMessage : AddScoped
@enduml
```

## Sequence Diagram

```plantuml
@startuml
 Participant "ServiceLocator" as service_locator
 Participant "ServiceCollection" as service_collection
 Participant "QueryBuilder<T>" as query_builder
 Participant "SortBuilder<T>" as sort_builder
 Participant "ExpressionTreeBuilder<T>" as expression_tree_builder
 Participant "PostBuildExpressionVisitor" as post_build_expression_visitor
 Participant "CaptureResultMessage" as capture_result_message

 sequence
  service_locator->>service_collection: TryAddSearchQueryExtensions()
  service_collection->>query_builder: TryAddTransient(IQueryBuilder<T>)
  service_collection->>sort_builder: TryAddTransient(ISortBuilder<T>)
  service_collection->>expression_tree_builder: TryAddTransient(IExpressionTreeBuilder<T>)
  service_collection->>post_build_expression_visitor: AddTransient(IPostBuildExpressionVisitor)
  service_collection->>capture_result_message: AddScoped(ICaptureResultMessage)

  note right: Configures the service collection with various shared services and components.
@enduml
```