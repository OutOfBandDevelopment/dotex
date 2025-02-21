Here is the documentation for the source code:

**ChainTypes.cs**

This file defines an internal enum called `ChainTypes` with two values: `AndAlso` and `OrElse`. These values are used to specify the logical operator for building expression chains.

**ExpressionExtensions.cs**

This file extends the `Expression` class with two static methods: `OrElse` and `AndAlso`. These methods are used to build expression chains.

```plantuml
@startuml
class ExpressionExtensions {
  - orElse(TModel, object) : Expression<Func<TModel, bool>>
  - AndAlso(TModel, object) : Expression<Func<TModel, bool>>
}
@enduml
```

**ExpressionTreeBuilder.cs**

This class is responsible for building and managing expression trees dynamically in the context of filtering data. It takes two optional parameters: `logger` and `capture`, used for logging and capturing result messages.

```plantuml
@startuml
class ExpressionTreeBuilder {
  + GetPredicateExpression(string, FilterParameter, StringComparison, bool) : Expression<Func<TModel, bool>>
  + BuildExpression(object, string, FilterParameter, bool) : Expression<Func<TModel, bool>>
  + GetPredicates(...) : IEnumerable<Expression<Func<TModel, bool>>>
  + PropertyExpressions() : IReadOnlyDictionary<string, Expression<Func<TModel, object>>>
  + GetSearchablePropertyNames() : IReadOnlyCollection<string>
  + GetSortablePropertyNames() : IReadOnlyCollection<string>
  + GetFilterablePropertyNames() : IReadOnlyCollection<string>
  + DefaultSortOrder() : IReadOnlyCollection<(string, OrderDirections)>
}
@enduml
```

**FilterParameterMessages.cs**

This file defines static strings for error messages related to filter parameter parsing.

```plantuml
@startuml
class FilterParameterMessages {
  - ParsedInput: string
  - ParsedInputCode: string
  - UnableToMapFilter: string
  - UnableToMapFilterCode: string
}
@enduml
```

**ParameterReplacerExpressionVisitor.cs**

This class is a visitor that replaces a parameter expression with a new parameter expression. It takes a `ParameterExpression` in its constructor and overrides the `VisitParameter` method.

```plantuml
@startuml
class ParameterReplacerExpressionVisitor {
  + VisitParameter(ParameterExpression) : Expression
}
@enduml
```

Note: The PlantUML diagrams are used to illustrate the relationships between classes and methods.