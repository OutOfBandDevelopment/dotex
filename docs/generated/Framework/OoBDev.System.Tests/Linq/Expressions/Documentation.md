# Expression Tree Builders and Visitors

## Expression Tree Builders

The Expression Tree Builders are used to analyze and manipulate LINQ expressions at runtime. They provide a way to transform and optimize LINQ expressions, making them more efficient and easier to understand.

### Class Diagram

```plantuml
@startuml
class ExpressionTreeBuilderTests {
    - TestContext: TestContext
    - GetFilterablePropertyNamesTest: method
    - GetSearchablePropertyNamesTest: method
    - GetSortablePropertyNamesTest: method
}

class ExpressionTreeBuilder<T> {
    - GetFilterablePropertyNames: method
    - GetSearchablePropertyNames: method
    - GetSortablePropertyNames: method
}

class TestContext {
    - WriteLine: method
    - AddResult: method
}

ExpressionTreeBuilderTests --* ExpressionTreeBuilder<T>
ExpressionTreeBuilder<T> --* TestContext
@enduml
```

### Expression Tree Builders Model

The Expression Tree Builders have three methods:

1. `GetFilterablePropertyNames`: Returns a list of filterable property names for the given type.
2. `GetSearchablePropertyNames`: Returns a list of searchable property names for the given type.
3. `GetSortablePropertyNames`: Returns a list of sortable property names for the given type.

### Sequence Diagram

```plantuml
@startuml
participant "ExpressionTreeBuilderTests"
participant "ExpressionTreeBuilder<T>"
participant "TestContext"

note "GetFilterablePropertyNamesTest" as note1
note1: Call ExpressionTreeBuilder<T>.GetFilterablePropertyNames() and assert result

note "GetSearchablePropertyNamesTest" as note2
note2: Call ExpressionTreeBuilder<T>.GetSearchablePropertyNames() and assert result

note "GetSortablePropertyNamesTest" as note3
note3: Call ExpressionTreeBuilder<T>.GetSortablePropertyNames() and assert result

ExpressionTreeBuilderTests -> ExpressionTreeBuilder<T>: Create new instance
ExpressionTreeBuilder<T> -> TestContext: Get filterable property names
ExpressionTreeBuilder<T> -> TestContext: Get searchable property names
ExpressionTreeBuilder<T> -> TestContext: Get sortable property names
TestContext -> ExpressionTreeBuilderTests: Return result
@enduml
```

## Visitors

The Visitors are used to analyze and transform LINQ expressions at runtime. They provide a way to add custom logic to LINQ expressions, making them more flexible and reusable.

### Class Diagram

```plantuml
@startuml
class SkipInstanceMethodOnNullExpressionVisitor {
    - Visit: method
}

class SkipMemberOnNullExpressionVisitor {
    - Visit: method
}

class StringComparisonReplacementExpressionVisitor {
    - Visit: method
}

class LambdaExpression {
    - Compile: method
}

SkipInstanceMethodOnNullExpressionVisitor --* LambdaExpression
SkipMemberOnNullExpressionVisitor --* LambdaExpression
StringComparisonReplacementExpressionVisitor --* LambdaExpression
LambdaExpression --* Expression<TDelegate>
@enduml
```

### Visitor Model

The Visitors have one method:

1. `Visit`: Visits the given LINQ expression and transforms it according to the visitor's logic.

### Sequence Diagram

```plantuml
@startuml
participant "SkipInstanceMethodOnNullExpressionVisitor"
participant "SkipMemberOnNullExpressionVisitor"
participant "StringComparisonReplacementExpressionVisitor"
participant "LambdaExpression"
participant "Expression<TDelegate>"

note "VisitTest" as note1
note1: Call SkipInstanceMethodOnNullExpressionVisitor.Visit() and assert result

note "VisitTest" as note2
note2: Call SkipMemberOnNullExpressionVisitor.Visit() and assert result

note "VisitTest" as note3
note3: Call StringComparisonReplacementExpressionVisitor.Visit() and assert result

SkipInstanceMethodOnNullExpressionVisitor -> LambdaExpression: Visit
SkipMemberOnNullExpressionVisitor -> LambdaExpression: Visit
StringComparisonReplacementExpressionVisitor -> LambdaExpression: Visit
LambdaExpression -> Expression<TDelegate>: Compile
Expression<TDelegate> -> SkipInstanceMethodOnNullExpressionVisitor: Execute
Expression<TDelegate> -> SkipMemberOnNullExpressionVisitor: Execute
Expression<TDelegate> -> StringComparisonReplacementExpressionVisitor: Execute
@enduml
```

## Tests

The tests for the Expression Tree Builders and Visitors are implemented using the Microsoft.VisualStudio.TestTools.UnitTesting framework.

### Expression Tree Builders Tests

```markdown
### Class: ExpressionTreeBuilderTests

* Method: GetFilterablePropertyNamesTest
	+ Tests GetFilterablePropertyNames method for different types
* Method: GetSearchablePropertyNamesTest
	+ Tests GetSearchablePropertyNames method for different types
* Method: GetSortablePropertyNamesTest
	+ Tests GetSortablePropertyNames method for different types
```

### Visitors Tests

```markdown
### Class: SkipInstanceMethodOnNullExpressionVisitorTests

* Method: VisitTest
	+ Tests SkipInstanceMethodOnNullExpressionVisitor.Visit method for different inputs
	+ Asserts result and expected result

