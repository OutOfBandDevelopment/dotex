using Microsoft.Extensions.Logging;
using System;

namespace OoBDev.System.Linq;

/// <summary>
/// Source-generated log messages for the LINQ query building classes.
/// </summary>
internal static partial class LinqLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "{QueryString} parsed to {Value} ({Type})")]
    internal static partial void ParsedValue(ILogger logger, object? queryString, object? value, Type? type);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Unable to map filter expression: {Expression} {ExpressionOperator} {QueryParameter} ({Type})")]
    internal static partial void UnmappedFilter(ILogger logger, object? expression, object? expressionOperator, object? queryParameter, Type? type);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "TryGetPropertyExpression: {Name}")]
    internal static partial void TryGetProperty(ILogger logger, string? name);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Replace: {Input} with {Replacement}")]
    internal static partial void Replace(ILogger logger, object? input, object? replacement);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Rewrite VisitMethodCall: {Method}({Args}) -> {Node} / {NodeType}")]
    internal static partial void RewriteMethodCall(ILogger logger, string method, int args, object node, Type nodeType);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Rewriting VisitMember: {Method} [{Args}] -> {Node}")]
    internal static partial void RewriteMember(ILogger logger, object method, object? args, object node);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Execute : {Query}")]
    internal static partial void Execute(ILogger logger, object query);

    [LoggerMessage(EventId = 8, Level = LogLevel.Debug, Message = "Intercepted by: {Interceptor}")]
    internal static partial void InterceptedBy(ILogger logger, object interceptor);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Build query for {SearchQuery}")]
    internal static partial void BuildQuery(ILogger logger, object searchQuery);

    [LoggerMessage(EventId = 10, Level = LogLevel.Warning, Message = "No filtering detected: {Query}: {SearchQuery}")]
    internal static partial void NoFiltering(ILogger logger, string? query, string? searchQuery);

    [LoggerMessage(EventId = 11, Level = LogLevel.Debug, Message = "Visited by: {Visitor}")]
    internal static partial void VisitedBy(ILogger logger, object visitor);

    [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "Execute (paged): {Query}")]
    internal static partial void ExecutePaged(ILogger logger, object query);

    [LoggerMessage(EventId = 13, Level = LogLevel.Warning, Message = "No filter mapped for property: {Name} => {Filter}")]
    internal static partial void NoFilterMapped(ILogger logger, object? name, object? filter);

    [LoggerMessage(EventId = 14, Level = LogLevel.Warning, Message = "Could not use properties: {UnmatchedKeys} as they are not on the model")]
    internal static partial void UnmatchedSortKeys(ILogger logger, string unmatchedKeys);

    [LoggerMessage(EventId = 15, Level = LogLevel.Information, Message = "Applying default sort for {Type}: {OrderBys}")]
    internal static partial void DefaultSort(ILogger logger, Type type, string orderBys);

    [LoggerMessage(EventId = 16, Level = LogLevel.Warning, Message = "Force sort by 0 applied for {Type}")]
    internal static partial void ForcedSort(ILogger logger, Type type);
}
