using OoBDev.Extensions.Reflection;
using OoBDev.System.ComponentModel.Search;
using OoBDev.System.Linq.Search;
using OoBDev.System.ResponseModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace OoBDev.System.Linq.Expressions;

/// <summary>
/// Provides functionality for building and managing expression trees dynamically in the context of filtering data.
/// </summary>
/// <typeparam name="TModel">The type of the data model.</typeparam>
/// <param name="logger">Optional logger for logging messages.</param>
/// <param name="capture">Optional result message capturer.</param>
public class ExpressionTreeBuilder<TModel>(
    ILogger<ExpressionTreeBuilder<TModel>>? logger = null,
    ICaptureResultMessage? capture = null
        ) : IExpressionTreeBuilder<TModel>
{
    private const string PropertyMap = nameof(PropertyMap);
    private const string PredicateMap = nameof(PredicateMap);

    private readonly ILogger _logger = logger ?? new ConsoleLogger<ExpressionTreeBuilder<TModel>>();
    private readonly ICaptureResultMessage _capture = capture ?? CaptureResultMessage.Default;

    /// <summary>
    /// Gets the predicate expression based on the provided parameters.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The filter parameter value.</param>
    /// <param name="stringComparison">The string comparison method.</param>
    /// <param name="isSearchTerm">Flag indicating if the value is a search term.</param>
    /// <returns>The predicate expression or null if not found.</returns>
    public virtual Expression<Func<TModel, bool>>? GetPredicateExpression(
        string name,
        FilterParameter value,
        StringComparison stringComparison,
        bool isSearchTerm
        ) =>
        TryGetPredicateExpression(name, value, out var expression, stringComparison, isSearchTerm) ? expression : null;

    /// <summary>
    /// Builds an expression by combining multiple predicate expressions using OR logic.
    /// </summary>
    /// <param name="queryParameter">The query parameter for filtering.</param>
    /// <param name="stringComparison">The string comparison method.</param>
    /// <param name="isSearchTerm">Flag indicating if the value is a search term.</param>
    /// <returns>The combined predicate expression or null if not applicable.</returns>
    public virtual Expression<Func<TModel, bool>>? BuildExpression(object? queryParameter, StringComparison stringComparison, bool isSearchTerm) =>
        ExpressionExtensions.OrElse(
            from searchExpression in GetSearchableExpressions(stringComparison)
            let builtExpression = BuildPredicate(
                searchExpression.property,
                searchExpression.expression,
                Operators.EqualTo,
                queryParameter,
                isSearchTerm
                )
            where builtExpression != null
            select builtExpression
        );

    private static KeyValuePair<string, Expression<Func<TModel, object>>>[] BuildExpressions() =>
        (
        from pi in typeof(TModel).GetProperties(ReflectionExtensions.PublicProperties)
        let exp = BuildExpression(pi)
        where exp != null
        select KeyValuePair.Create(pi.Name, exp)
        ).ToArray();

    private IEnumerable<Expression<Func<TModel, bool>>?> GetPredicates(
        string scope,
        Expression<Func<TModel, object>>? expression,
        FilterParameter? search,
        bool isSearchTerm
        )
    {
        if (search == null) yield break;

        yield return BuildPredicate(scope, expression, Operators.EqualTo, search.EqualTo, isSearchTerm);
        yield return BuildPredicate(scope, expression, Operators.NotEqualTo, search.NotEqualTo, isSearchTerm);

        yield return BuildPredicate(scope, expression, Operators.InSet, search.InSet, isSearchTerm);

        yield return BuildPredicate(scope, expression, Operators.LessThan, search.LessThan, isSearchTerm);
        yield return BuildPredicate(scope, expression, Operators.LessThanOrEqualTo, search.LessThanOrEqualTo, isSearchTerm);

        yield return BuildPredicate(scope, expression, Operators.GreaterThan, search.GreaterThan, isSearchTerm);
        yield return BuildPredicate(scope, expression, Operators.GreaterThanOrEqualTo, search.GreaterThanOrEqualTo, isSearchTerm);

        //TODO: add support for these operations
        //yield return BuildPredicate(scope, expression, Operators.Ands, search.Ands, isSearchTerm);
        //yield return BuildPredicate(scope, expression, Operators.Ors, search.Ors, isSearchTerm);
        //yield return BuildPredicate(scope, expression, Operators.OrNull, search.OrNull, isSearchTerm);
    }

    private Expression<Func<TModel, bool>>? BuildPredicate(
        string scope,
        Expression<Func<TModel, object>>? expression,
        FilterParameter? search,
        bool isSearchTerm
        ) => GetPredicates(scope, expression, search, isSearchTerm).AndAlso();

    private Expression<Func<TModel, bool>>? BuildPredicate(
        string scope,
        Expression<Func<TModel, object>>? expression,
        Operators expressionOperator,
        object? queryParameter,
        bool isSearchTerm
        )
    {
        if (expression == null || queryParameter == null) return null;
        if (queryParameter is FilterParameter search)
            return BuildPredicate(scope, expression, search, isSearchTerm);

        var unwrapped = expression.Body;
        if (expression.Body.NodeType is ExpressionType.Convert or
            ExpressionType.ConvertChecked)
        {
            if (expression.Body is UnaryExpression unary)
            {
                unwrapped = unary.Operand;
            }
        }

        if (queryParameter is not string)
        {
            queryParameter = queryParameter switch
            {
                IEnumerable<char> charArray => new string(charArray.ToArray()),
                Array _ => queryParameter,
                IEnumerable enumerable => enumerable.OfType<object>().ToArray(),
                _ => Convert.ToString(queryParameter)
            };
        }

        var queryParameterType = queryParameter?.GetType() ??
            throw new NullReferenceException($"{nameof(queryParameter)} may not be null");

        if (expressionOperator == Operators.InSet)
        {
            if (queryParameterType.IsArray) //TODO: should support IEnumerable<> as well
            {
                var elementType = queryParameterType.GetElementType();
                if (elementType == unwrapped.Type)
                {
                    var enumerable = typeof(IEnumerable<>).MakeGenericType(elementType);

                    Expression<Func<object, bool>> model = e => Enumerable.Contains((object[])queryParameter, e);
                    var containsMethod = (model.Body as MethodCallExpression)?.Method.GetGenericMethodDefinition().MakeGenericMethod(elementType);
                    if (containsMethod != null)
                    {
                        var containsCall = Expression.Call(method: containsMethod, Expression.Constant(queryParameter), unwrapped);
                        var parameter = Expression.Parameter(typeof(TModel), "n");
                        var replaced = new ParameterReplacerExpressionVisitor(parameter).Visit(containsCall);
                        var lambda = Expression.Lambda<Func<TModel, bool>>(replaced, parameter);
                        return lambda;
                    }
                }
                else
                {
                    var safeArray = unwrapped.Type.MakeSafeArray((Array)queryParameter, _capture);
                    if (safeArray != null)
                    {
                        var recursive = BuildPredicate(scope, expression, expressionOperator, safeArray, isSearchTerm);
                        if (recursive != null) return recursive;
                    }
                }
            }
            throw new NotSupportedException($"{nameof(expressionOperator)}: {expressionOperator} does not support \"{queryParameterType}\"");

        }

        var isElementSet = IsElementSet(unwrapped.Type, out var unwrappedElementType);

        if (queryParameter is string queryString && queryString.Length > 0) //TODO: should be "like"
        {
            if (queryString[..1] == "!")
            {
                return BuildPredicate(scope, expression, Operators.NotEqualTo, queryString[1..], isSearchTerm);
            }
            else if (unwrappedElementType == typeof(string))
            {
                if (expressionOperator == Operators.NotEqualTo)
                {
                    var eq = BuildPredicate(scope, expression, Operators.EqualTo, queryParameter, isSearchTerm) ??
                        throw new NotSupportedException(nameof(Operators.NotEqualTo));
                    var predicate = Expression.Not(eq.Body);
                    var parameter = Expression.Parameter(typeof(TModel), "n");
                    var replaced = new ParameterReplacerExpressionVisitor(parameter).Visit(predicate);
                    var lambda = Expression.Lambda<Func<TModel, bool>>(replaced, parameter);
                    return lambda;
                }
                else
                {
                    if (queryString == "*") return null; // if only wildcard then exclude the predicate

                    var (method, queryValue) = (queryString[..1], queryString[^1..]) switch
                    {
                        ("*", "*") => (
                            typeof(string).GetInstanceMethod(nameof(string.Contains), typeof(string)),
                            queryString[1..^1]
                            ),

                        ("*", _) => (
                            typeof(string).GetInstanceMethod(nameof(string.EndsWith), typeof(string)),
                            queryString[1..]
                            ),

                        (_, "*") => (
                            typeof(string).GetInstanceMethod(nameof(string.StartsWith), typeof(string)),
                            queryString[..^1]
                            ),

                        (_, _) => (
                            typeof(string).GetInstanceMethod(nameof(string.Equals), typeof(string)),
                            queryString
                            )
                    };
                    if (method == null)
                    {
                        //  TODO: make null safe
                        throw new NotSupportedException("Method not defined");
                    }

                    if (isElementSet)
                    {
                        //Note: target is a set

                        Expression<Func<object[], bool>> example = i => i.Any(_ => true);
                        var anyMethod = ((MethodCallExpression)example.Body).Method
                            .GetGenericMethodDefinition()
                            .MakeGenericMethod(unwrappedElementType)
                            ;

                        var childParameter = Expression.Parameter(unwrappedElementType, "child");
                        var childCall = Expression.Call(childParameter, method, Expression.Constant(queryValue));
                        var childLambda = Expression.Lambda(childCall, childParameter);

                        var modelCall = Expression.Call(anyMethod, unwrapped, childLambda);

                        var parameter = Expression.Parameter(typeof(TModel), "n");
                        var replaced = new ParameterReplacerExpressionVisitor(parameter).Visit(modelCall);
                        var lambda = Expression.Lambda<Func<TModel, bool>>(replaced, parameter);
                        return lambda;
                    }
                    else
                    {
                        var predicate = Expression.Call(unwrapped, method, Expression.Constant(queryValue));

                        var parameter = Expression.Parameter(typeof(TModel), "n");
                        var replaced = new ParameterReplacerExpressionVisitor(parameter).Visit(predicate);
                        var lambda = Expression.Lambda<Func<TModel, bool>>(replaced, parameter);
                        return lambda;
                    }
                }
            }
            else if (unwrapped.Type.TryParse(queryString, out var value, _capture))
            {
                _capture.Publish(new ResultMessage
                {
                    Message = FilterParameterMessages.ParsedInput,
                    MessageCode = FilterParameterMessages.ParsedInputCode,
                    Context = scope,
                    MetaData = new
                    {
                        source = queryString,
                        target = value,
                        targetType = value?.GetType()?.FullName,
                    },
                });
                _logger.LogWarning(
                    $"{{{nameof(queryString)}}} parsed to {{{nameof(value)}}} ({{type}})",
                    queryString,
                    value,
                    value?.GetType()
                    );
                queryParameter = value;
            }
        }

        if (queryParameter != null && unwrapped.Type.IsAssignableFrom(queryParameter.GetType()))
        {
            //TODO: needs to be a bit more creative.  type casting not supported
            var parameter = Expression.Parameter(typeof(TModel), "n");
            var predicate = BuildBinaryExpression(expressionOperator, unwrapped, queryParameter);
            var replaced = new ParameterReplacerExpressionVisitor(parameter).Visit(predicate);
            var lambda = Expression.Lambda<Func<TModel, bool>>(replaced, parameter);
            return lambda;
        }
        else
        {
            if (!isSearchTerm)
            {
                _capture.Publish(new ResultMessage
                {
                    Message = FilterParameterMessages.UnableToMapFilter,
                    MessageCode = FilterParameterMessages.UnableToMapFilterCode,
                    Context = scope,
                    MetaData = new
                    {
                        source = queryParameter,
                    },
                });
                _logger.LogWarning(
                    $"Unable to map filter expression: {{{nameof(expression)}}} {{{nameof(expressionOperator)}}} {{{nameof(queryParameter)}}} ({{type}})",
                    expression,
                    expressionOperator,
                    queryParameter,
                    queryParameter?.GetType()
                    );
#if DEBUG
                throw new NotSupportedException($"Filter not mapped: {expression} {expressionOperator} {queryParameter}");
#endif
            }

            return default;
        }
    }

    private static bool IsElementSet(Type type, out Type elementType)
    {
        if (type != typeof(string))
        {
            if (type.IsArray)
            {
                elementType = type.GetElementType() ?? typeof(object);
                return true;
            }
            else if (type.IsAssignableTo(typeof(IEnumerable)))
            {
                var query = from i in type.GetInterfaces()
                            where i.IsGenericType
                            where i.GetGenericTypeDefinition().IsAssignableTo(typeof(IEnumerable<>))
                            select i.GetGenericArguments()[0];

                elementType = query.FirstOrDefault() ?? typeof(object);
                return true;
            }
        }
        elementType = type;
        return false;
    }

    /// <summary>
    /// Builds the expressions for the properties in the data model.
    /// </summary>
    /// <returns>The dictionary containing property names and their corresponding expressions.</returns>
    public virtual IReadOnlyDictionary<string, Expression<Func<TModel, object>>> PropertyExpressions() =>
        new Dictionary<string, Expression<Func<TModel, object>>>(BuildExpressions(), StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Gets the searchable property names in the data model.
    /// </summary>
    /// <returns>The collection of searchable property names.</returns>
    public virtual IReadOnlyCollection<string> GetSearchablePropertyNames()
    {
        var modelType = typeof(TModel);

        var properties = modelType
            .GetCustomAttributes<SearchableAttribute>().Select(a => a.TargetName)
            .Concat(from prop in modelType.GetProperties(ReflectionExtensions.PublicProperties)
                    from attrib in prop.GetCustomAttributes<SearchableAttribute>()
                    select attrib.TargetName ?? prop.Name)
            .Distinct();

        if (!properties.Any())
        {
            properties = modelType.GetProperties(ReflectionExtensions.PublicProperties).Select(p => p.Name).Distinct();
        }

        var notSearchable = modelType
            .GetCustomAttributes<NotSearchableAttribute>().Select(a => a.TargetName)
            .Concat(from prop in modelType.GetProperties(ReflectionExtensions.PublicProperties)
                    from attrib in prop.GetCustomAttributes<NotSearchableAttribute>()
                    select attrib.TargetName ?? prop.Name)
            .Distinct();

        var results = properties
            .Except(notSearchable)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Cast<string>()
            .ToArray();

        return results;
    }

    /// <summary>
    /// Gets the sortable property names in the data model.
    /// </summary>
    /// <returns>The collection of sortable property names.</returns>
    public virtual IReadOnlyCollection<string> GetSortablePropertyNames()
    {
        var modelType = typeof(TModel);

        var properties = modelType
            .GetCustomAttributes<SearchableAttribute>().Select(a => a.TargetName)
            .Concat(from prop in modelType.GetProperties(ReflectionExtensions.PublicProperties)
                    select prop.Name)
            .Distinct();

        var notSearchable = modelType
            .GetCustomAttributes<NotSortableAttribute>().Select(a => a.TargetName)
            .Concat(from prop in modelType.GetProperties(ReflectionExtensions.PublicProperties)
                    from attrib in prop.GetCustomAttributes<NotSortableAttribute>()
                    select attrib.TargetName ?? prop.Name)
            .Distinct();

        var results = properties
            .Except(notSearchable)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Cast<string>()
            .ToArray();

        return results;
    }

    /// <summary>
    /// Gets the filterable property names in the data model.
    /// </summary>
    /// <returns>The collection of filterable property names.</returns>
    public virtual IReadOnlyCollection<string> GetFilterablePropertyNames()
    {
        var modelType = typeof(TModel);

        var searchableProperties = modelType
            .GetCustomAttributes<SearchableAttribute>().Select(a => a.TargetName)
            .Concat(from prop in modelType.GetProperties(ReflectionExtensions.PublicProperties)
                    select prop.Name)
            .Distinct();

        var properties = modelType
            .GetCustomAttributes<FilterableAttribute>().Select(a => a.TargetName)
            .Concat(from prop in modelType.GetProperties(ReflectionExtensions.PublicProperties)
                    select prop.Name)
            .Distinct();

        var notSearchable = modelType
            .GetCustomAttributes<NotFilterableAttribute>().Select(a => a.TargetName)
            .Concat(from prop in modelType.GetProperties(ReflectionExtensions.PublicProperties)
                    from attrib in prop.GetCustomAttributes<NotFilterableAttribute>()
                    select attrib.TargetName ?? prop.Name)
            .Distinct();

        var results =
            searchableProperties
            .Concat(properties)
            .Except(notSearchable)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Cast<string>()
            .ToArray();

        return results;
    }

    /// <summary>
    /// Returns the default sort order based on attributes.
    /// </summary>
    /// <returns>The default sort order as a collection of column names and directions.</returns>
    public virtual IReadOnlyCollection<(string column, OrderDirections direction)> DefaultSortOrder() =>
        (
            from attribute in typeof(TModel).GetCustomAttributes<DefaultSortAttribute>()
            where !string.IsNullOrWhiteSpace(attribute.TargetName)
            select (name: attribute.TargetName, attribute.Order, attribute.Priority)
        ).Concat(
            from prop in typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
            let attribute = prop.GetCustomAttribute<DefaultSortAttribute>()
            where attribute != null
            select (name: attribute.TargetName ?? prop.Name, attribute.Order, attribute.Priority)
        ).OrderBy(p => p.Priority).Select(o => (o.name, o.Order))
        .ToArray();

    private static BinaryExpression BuildBinaryExpression(Operators expressionOperator, Expression left, object right) =>
        BuildBinaryExpression(expressionOperator, left, right as Expression ?? Expression.Constant(right));
    private static BinaryExpression BuildBinaryExpression(Operators expressionOperator, Expression left, Expression right)
    {
        if (left.Type != right.Type)
        {
            right = Expression.Convert(right, left.Type);
        }

        var result = expressionOperator switch
        {
            Operators.EqualTo => Expression.Equal(left, right),
            Operators.NotEqualTo => Expression.NotEqual(left, right),

            Operators.LessThan => Expression.LessThan(left, right),
            Operators.LessThanOrEqualTo => Expression.LessThanOrEqual(left, right),
            Operators.GreaterThan => Expression.GreaterThan(left, right),
            Operators.GreaterThanOrEqualTo => Expression.GreaterThanOrEqual(left, right),

            _ => throw new NotSupportedException($"{expressionOperator} is not supported"),
        };
        return result;
    }

    private (string property, Expression<Func<TModel, object>> expression)[] GetSearchableExpressions(StringComparison stringComparison) =>
        (
        from property in GetSearchablePropertyNames()
        let expression = TryGetPropertyExpression(property, out var exp, stringComparison) ? exp : null
        where expression != null
        select (property, expression)
        ).ToArray();

    private static Expression<Func<TModel, object>>? BuildExpression(PropertyInfo? prop)
    {
        if (prop == null) return null;
        var parameter = Expression.Parameter(typeof(TModel), "n");
        Expression property = Expression.Property(parameter, prop);

        if (!prop.PropertyType.IsClass)
            property = Expression.Convert(property, typeof(object));

        var exp = Expression.Lambda<Func<TModel, object>>(property, parameter);

        return exp;
    }

    private static object? SimplifyValue(object? value) =>
        value switch
        {
            //TODO: consider this being a type converter on FilterParameter instead
            FilterParameter filter => ExpressionTreeBuilder<TModel>.SimplifyValue(filter.EqualTo),
            JsonElement element when element.ValueKind == JsonValueKind.String => element.GetString(),
            JsonElement element when element.ValueKind == JsonValueKind.Number => element.GetDouble(),
            JsonElement element when element.ValueKind == JsonValueKind.Array => element.EnumerateArray().Select(i => ExpressionTreeBuilder<TModel>.SimplifyValue(i)).ToArray(),
            _ => value,
        };

    private bool TryGetPredicateExpression(
        string name,
        FilterParameter value,
        out Expression<Func<TModel, bool>>? expression,
        StringComparison stringComparison,
        bool isSearchTerm
        )
    {
        var modelType = typeof(TModel);

        expression = modelType.GetStaticMethod(PredicateMap, typeof(string), typeof(FilterParameter))
            ?.Invoke(null, [name, value]) as Expression<Func<TModel, bool>>;

        expression ??= modelType.GetStaticMethod(PredicateMap, typeof(string), typeof(FilterParameter), typeof(StringComparison))
            ?.Invoke(null, [name, value, stringComparison]) as Expression<Func<TModel, bool>>;

        if (value.EqualTo != null)
        {
            //TODO: we should have logic to enumerate the requested type instead of just assuming the type based on the json type.
            var simple = ExpressionTreeBuilder<TModel>.SimplifyValue(value);
            if (simple != null)
            {
                expression = modelType.GetStaticMethod(PredicateMap, typeof(string), typeof(object))
                    ?.Invoke(null, [name, simple]) as Expression<Func<TModel, bool>>;

                expression ??= modelType.GetStaticMethod(PredicateMap, typeof(string), typeof(object), typeof(StringComparison))
                    ?.Invoke(null, [name, simple, stringComparison]) as Expression<Func<TModel, bool>>;
            }
        }

        //TODO: should I add support to unroll the searchOption?

        expression ??= BuildPredicate(name, GetPropertyExpression(name, stringComparison), value, isSearchTerm);

        return expression != null;
    }

    private Expression<Func<TModel, object>>? GetPropertyExpression(
        string name,
        StringComparison stringComparison
        ) =>
        TryGetPropertyExpression(name, out var expression, stringComparison) ?
            expression : null;

    private bool TryGetPropertyExpression(
        string name,
        out Expression<Func<TModel, object>>? expression,
        StringComparison stringComparison
        )
    {
        var modelType = typeof(TModel);

        expression = modelType.GetStaticMethod(PropertyMap, typeof(string))
            ?.Invoke(null, new[] { name }) as Expression<Func<TModel, object>>;

        expression ??= modelType.GetStaticMethod(PropertyMap, typeof(string), typeof(StringComparison))
            ?.Invoke(null, [name, stringComparison]) as Expression<Func<TModel, object>>;

        expression ??= BuildExpression(
            modelType.GetProperties(ReflectionExtensions.PublicProperties)
            .FirstOrDefault(pi => string.Equals(pi.Name, name, stringComparison))
            );

        if (expression == null)
        {
            _logger.LogInformation($"{nameof(TryGetPropertyExpression)}: {{{nameof(name)}}}", name);
        }

        return expression != null;
    }
}
