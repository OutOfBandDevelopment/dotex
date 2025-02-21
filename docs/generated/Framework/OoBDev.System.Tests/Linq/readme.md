**README File**

**Summary**

The provided source files are part of a unit testing project for a queryable extension framework. The tests cover various functionality, including filtering, paginating, sorting, and searching data. The framework allows for querying a list of objects based on various criteria, such as equality, greater than, less than, and regular expressions.

**Design and Architecture**

The framework uses a combination of design patterns and architectural patterns to achieve its functionality. The primary architectural pattern used is the Repository pattern, where a repository class acts as an intermediary between the data source and the business logic.

The framework also employs various design patterns, including:

1. **Builder pattern**: Used in the `TestDataBuilder` class to construct test data based on a given type and seed value.
2. **Factory pattern**: Used in the `TestDataBuilder` class to create instances of a given type based on a constructor with an integer parameter.
3. **Visitor pattern**: Used in the `CaptureResultMessage` class to capture and store query results.

** Component Diagram**

Using PlantUML, the component diagram for the queryable extension framework is as follows:

```plantuml
@startuml
component "QueryProvider" {
  component "QueryBuilder"
  component "SearchQuery"
}

component "SearchQuery" {
  component "Filter"
  component "OrderBy"
}

component "Filter" {
  component "ExpressionOperator"
}

component "OrderBy" {
  component "OrderDirection"
}

component "TestContext" {
  component "TestResults"
}

component "CaptureResultMessage" {
  component "Results" {
    component "Rows" {
      component "Row" {
        component "KeyValue"
      }
    }
  }
}

component "TestDataBuilder" {
  component "Factory" {
    component "Constructor" {
      component "Parameter" {
        component "Int"
      }
    }
  }
}

component "OoBDev.System.Linq.Search" {
  component "SearchQuery<T>"
}

component "OoBDev.System.Linq.Expressions" {
  component "ExpressionOperator"
}

component "OoBDev.TestUtilities" {
  component "TestContext"
}

component "OoBDev.System.Tests.Linq" {
  component "QueryableExtensionsTests"
}

OoBDev.System.Linq.Search -- left -- SearchQuery
OoBDev.System.Linq.Expressions -- left -- ExpressionOperator
OoBDev.TestUtilities -- left -- TestContext
OoBDev.System.Tests.Linq -- left -- QueryableExtensionsTests
TestDataBuilder -- right -- Factory
Factory -- right -- Constructor
Constructor -- right -- Parameter
Parameter -- right -- Int
CaptureResultMessage -- right -- Results
Results -- right -- Rows
Rows -- right -- Row
Row -- right -- KeyValue

@enduml
```

This component diagram illustrates the relationships between the different components of the queryable extension framework and its dependencies.