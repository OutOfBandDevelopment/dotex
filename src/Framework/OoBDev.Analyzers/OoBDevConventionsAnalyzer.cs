using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace OoBDev.Analyzers;

/// <summary>Enforces OoBDev coding conventions that the compiler and stock analyzers do not cover.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OoBDevConventionsAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Use <c>TimeProvider</c> rather than reading the system clock directly.</summary>
    public static readonly DiagnosticDescriptor UseTimeProvider = new(
        "OOB0001",
        "Use TimeProvider",
        "Use an injected TimeProvider instead of '{0}'",
        "OoBDev.Conventions",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>Provider keys are lower-case kebab-case.</summary>
    public static readonly DiagnosticDescriptor KebabCaseKey = new(
        "OOB0002",
        "Provider key must be kebab-case",
        "Keyed service key '{0}' must be lower-case kebab-case (letters, digits and single hyphens)",
        "OoBDev.Conventions",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>Third-party IoC containers and logging libraries are rejected.</summary>
    public static readonly DiagnosticDescriptor BannedLibrary = new(
        "OOB0003",
        "Third-party IoC or logging library",
        "'{0}' is a rejected third-party library; use Microsoft.Extensions.* instead",
        "OoBDev.Conventions",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly string[] BannedNamespaces =
    [
        "Autofac", "Serilog", "NLog", "log4net", "Ninject", "SimpleInjector", "StructureMap", "Castle.Windsor", "Polly",
    ];

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(UseTimeProvider, KebabCaseKey, BannedLibrary);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeUsing, SyntaxKind.UsingDirective);
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        var access = (MemberAccessExpressionSyntax)context.Node;
        var name = access.Name.Identifier.ValueText;
        if (name is not ("Now" or "UtcNow"))
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(access, context.CancellationToken).Symbol is not IPropertySymbol property)
        {
            return;
        }

        var type = property.ContainingType.ToDisplayString();
        if (type is "System.DateTime" or "System.DateTimeOffset")
        {
            context.ReportDiagnostic(Diagnostic.Create(UseTimeProvider, access.GetLocation(), $"{property.ContainingType.Name}.{name}"));
        }
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var methodName = invocation.Expression switch
        {
            MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
            GenericNameSyntax generic => generic.Identifier.ValueText,
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            _ => null,
        };

        if (methodName is null
            || !(methodName.StartsWith("AddKeyed", StringComparison.Ordinal) || methodName.StartsWith("TryAddKeyed", StringComparison.Ordinal)))
        {
            return;
        }

        var keyArgument = invocation.ArgumentList.Arguments.FirstOrDefault(
            a => a.Expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression));
        if (keyArgument is null || context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol method)
        {
            return;
        }

        if (method.ContainingType.Name is not ("ServiceCollectionServiceExtensions" or "ServiceCollectionDescriptorExtensions"))
        {
            return;
        }

        var key = ((LiteralExpressionSyntax)keyArgument.Expression).Token.ValueText;
        if (!IsKebabCase(key))
        {
            context.ReportDiagnostic(Diagnostic.Create(KebabCaseKey, keyArgument.GetLocation(), key));
        }
    }

    private static void AnalyzeUsing(SyntaxNodeAnalysisContext context)
    {
        var directive = (UsingDirectiveSyntax)context.Node;
        var name = directive.Name?.ToString();
        if (name is null)
        {
            return;
        }

        foreach (var banned in BannedNamespaces)
        {
            if (name == banned || name.StartsWith(banned + ".", StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(BannedLibrary, directive.GetLocation(), banned));
                return;
            }
        }
    }

    internal static bool IsKebabCase(string value)
    {
        if (value.Length == 0 || value[0] == '-' || value[value.Length - 1] == '-')
        {
            return false;
        }

        var previousHyphen = false;
        foreach (var c in value)
        {
            var hyphen = c == '-';
            if (!(hyphen || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) || (hyphen && previousHyphen))
            {
                return false;
            }

            previousHyphen = hyphen;
        }

        return true;
    }
}
