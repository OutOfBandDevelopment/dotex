As a senior software engineer/solutions architect, here are my thoughts on the provided source code and suggestions for improvement:

1. **Organization and Naming**:
	* The classes in the OoBDev.System.Linq.Expressions namespace are related to expression visitors. Consider creating a sub-namespace for each visitor's concern (e.g., SkipInstanceMethodOnNullExpressionVisitor could be in OoBDev.System.Linq.Expressions.InstanceMethodVisitors).
	* Class names should be descriptive and follow a consistent naming convention (e.g., `SkipInstanceMethodOnNullExpressionVisitor` could be `InstanceMethodNullCheckingVisitor`).
2. **Code Duplication**:
	* In `SkipMemberOnNullExpressionVisitor`, the `VisitMethodCall` and `VisitLambda` methods have similar logic. Consider extracting a common method or utility class to avoid duplication.
3. **Conditionals and Control Flow**:
	* In `SkipMemberOnNullExpressionVisitor`, there are multiple `if` statements and conditionals that can make the code harder to follow. Consider refactoring to use separate methods or utility classes for each specific condition.
4. **Logic Extraction**:
	* In `StringComparisonReplacementExpressionVisitor`, the `VisitMethodCall` and `VisitBinary` methods contain complex logic. Consider extracting separate methods or classes for each specific replacement logic (e.g., `ReplaceStringEquals` and `ReplaceStringMethod`).
5. **Type Casting and Creational Patterns**:
	* In `SkipMemberOnNullExpressionVisitor`, there are instances of explicit type casting (`Expression.AndAlso`) and complex expression creation. Consider using fluent API or utility classes to simplify the code.
6. **Error Handling and Logging**:
	* In `StringComparisonReplacementExpressionVisitor`, there are some error handling and logging mechanisms. Consider extracting a dedicated logging utility class for better maintainability.
7. **Extensions and Utility Methods**:
	* In `SkipInstanceMethodOnNullExpressionVisitor`, there is a utility method `Expression.AndAlso`. Consider extracting it into an extension method or a utility class for better reusability.

Some specific changes I would suggest:

In `SkipInstanceMethodOnNullExpressionVisitor`:
```csharp
protected override Expression VisitMethodCall(MethodCallExpression node)
{
    return node.Object == null || node.Method.IsStatic
        ? base.VisitMethodCall(node)
        : node.Object != null
            ? node
            : Expression.AndAlso(Expression.NotEqual(Expression.Constant(null, node.Object.Type), node.Object), node);
}
```

In `SkipMemberOnNullExpressionVisitor`:
```csharp
protected override Expression VisitMethodCall(MethodCallExpression node)
{
    if (node.Method.IsStatic || node.Object == null || ShouldSkip(node.Method, node.Arguments))
    {
        return base.VisitMethodCall(node);
    }

    node = node.Object is MemberExpression member
        ? ReplaceMemberExpression(member, node.Method)
        : node;

    return node;
}

bool ShouldSkip(MethodInfo method, ReadOnlyCollection<Expression> arguments)
{
    // return true if the method should be skipped based on its signature and arguments
}
```

In `StringComparisonReplacementExpressionVisitor`:
```csharp
protected override Expression VisitMethodCall(MethodCallExpression input)
{
    var method = GetReplacingMethod(input.Method, stringComparison);
    if (method != null)
    {
        return Expression.Call(input.Object, method, input.Arguments.Concat(new[] { Expression.Constant(stringComparison) }));
    }

    return base.VisitMethodCall(input);
}

MethodInfo GetReplacingMethod(MethodInfo originalMethod, StringComparison stringComparison)
{
    // return the replacing method based on the original method and string comparison
}
```

These changes aim to improve code organization, reduce duplication, and simplify complex logic. However, it's essential to consider the specific requirements and constraints of the project to ensure that these changes do not introduce unintended side effects.