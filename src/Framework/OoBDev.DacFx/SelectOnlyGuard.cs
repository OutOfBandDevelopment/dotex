using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace OoBDev.DacFx;

/// <summary>
/// Accepts a T-SQL text only when it is exactly one plain SELECT. It is a first line of defence for a query runner;
/// the real guard is a login that can only read.
/// </summary>
public static class SelectOnlyGuard
{
    /// <summary>Checks the text.</summary>
    /// <param name="sql">The T-SQL text to check.</param>
    /// <returns>Allowed, or the reason the text is refused.</returns>
    public static SelectOnlyResult Check(string? sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return SelectOnlyResult.Refuse("The query is empty.");
        }

        var parser = new TSql170Parser(initialQuotedIdentifiers: true);
        using var reader = new StringReader(sql);
        var fragment = parser.Parse(reader, out IList<ParseError> errors);
        if (errors.Count > 0)
        {
            return SelectOnlyResult.Refuse($"The query does not parse: {errors[0].Message} (line {errors[0].Line}).");
        }

        if (fragment is not TSqlScript script || script.Batches.Count != 1 || script.Batches[0].Statements.Count != 1)
        {
            return SelectOnlyResult.Refuse("Exactly one statement is allowed.");
        }

        if (script.Batches[0].Statements[0] is not SelectStatement select)
        {
            return SelectOnlyResult.Refuse("Only a SELECT statement is allowed.");
        }

        var finder = new ForbiddenFinder();
        select.Accept(finder);
        return finder.Reason is null ? SelectOnlyResult.Allow() : SelectOnlyResult.Refuse(finder.Reason);
    }

    private sealed class ForbiddenFinder : TSqlFragmentVisitor
    {
        public string? Reason { get; private set; }

        public override void Visit(SelectStatement node)
        {
            if (node.Into is not null)
            {
                Reason ??= "SELECT INTO is not allowed.";
            }
        }

        public override void Visit(OpenRowsetTableReference node) => Reason ??= "OPENROWSET is not allowed.";

        public override void Visit(AdHocTableReference node) => Reason ??= "OPENDATASOURCE is not allowed.";

        public override void Visit(OpenQueryTableReference node) => Reason ??= "OPENQUERY is not allowed.";

        public override void Visit(BulkOpenRowset node) => Reason ??= "OPENROWSET BULK is not allowed.";

        public override void Visit(VariableTableReference node) => Reason ??= "Table variables are not allowed.";

        public override void Visit(SelectSetVariable node) => Reason ??= "Assigning a variable in a SELECT is not allowed.";
    }
}
