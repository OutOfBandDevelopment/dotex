**QueryBuilderTests Documentation**

**Overview**
===============

This document provides an overview of the `QueryBuilderTests` class and its associated source code. The class is responsible for testing the functionality of the `QueryBuilder` class, which is used to execute queries on a data source.

**Class Diagram**
================

```plantuml
@startuml
class QueryBuilderTests {
  - TestContext: TestContext
  + ExecuteByTest_IQueryable()
}

class QueryBuilder {
  + ExecuteBy(IQueryable<T>, SearchQuery query)
}

class TestTargetModel {
  - Name: string
  - Email: string
}

interface ISortBuilder<T> {
  IQueryable<T> SortBy(IQueryable<T>, ISortQuery query, IExpressionTreeBuilder<T>, StringComparison comparison)
}

interface IExpressionTreeBuilder<T> {
  Expression<Func<T, bool>> BuildExpression(object searchCriteria, StringComparison comparison, bool ignoreCase)
  Expression<Func<T, bool>>? GetPredicateExpression(string property, FilterParameter filter, StringComparison comparison, bool negated)
}

interface IQueryBuilder<T> {
  IQueryable<T> ExecuteBy(IQueryable<T>, SearchQuery query)
}

@enduml
```

**Component Model**
================

```plantuml
@startuml
component "QueryBuilderTests" {
  component "QueryBuilder"
  component "TestTargetModel"
}

component "QueryBuilder" {
  interface "ISortBuilder"
  interface "IExpressionTreeBuilder"
}

component "ISortBuilder" {
  operation "SortBy"
}

component "IExpressionTreeBuilder" {
  operation "BuildExpression"
  operation "GetPredicateExpression"
}

component "TestTargetModel" {
  attribute "Name"
  attribute "Email"
}

@enduml
```

**Sequence Diagram**
================

```plantuml
@startuml
actor "QueryBuilderTests"
 participant "QueryBuilder"
 participant "ISortBuilder"
 participant "IExpressionTreeBuilder"
 participant "SearchQuery"
 participant "TestDataBuilder"
 participant "TestContext"

QueryBuildTests ->> QueryBuilder : ExecuteBy(IQueryable<T>, SearchQuery query)
QueryBuilder ->> ISortBuilder : SortBy(IQueryable<T>, ISortQuery query, IExpressionTreeBuilder<T>, StringComparison comparison)
ISortBuilder ->> IExpressionTreeBuilder : BuildExpression
IExpressionTreeBuilder ->> QueryBuilder : BuildExpression
QueryBuilder ->> TestContext : Write

@enduml
```

**Test Description**
--------------------

The `ExecuteByTest_IQueryable` method tests the `QueryBuilder` class by executing a query on a test data source. The test data source is created using the `TestDataBuilder` class, which generates a list of `TestTargetModel` objects. The query is defined in the `SearchQuery` class, which contains the search term, filter, and order by criteria.

The test method sets up the query builder and executes the query using the `ExecuteBy` method. The query result is then checked to ensure it is not null. The test also captures any logging messages and writes them to the console.

**Test Context**
----------------

The `TestContext` class is used to provide context for the test. It contains methods for writing output to the console and for capturing logging messages.

**Test Categories**
-------------------

The test is categorized as a unit test and belongs to the "Unit" category.

**Dependences**
--------------

The `QueryBuilderTests` class depends on the following classes and interfaces:

* OoBDev.System.Linq.Expressions
* OoBDev.System.Linq.Search
* OoBDev.System.ResponseModel
* OoBDev.System.Tests.Linq.TestTargets
* OoBDev.TestUtilities
* Microsoft.VisualStudio.TestTools.UnitTesting
* Moq
* System.Linq
* System.Linq.Expressions