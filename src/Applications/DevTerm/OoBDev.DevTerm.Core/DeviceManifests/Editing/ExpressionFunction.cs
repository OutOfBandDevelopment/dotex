namespace OoBDev.DevTerm.DeviceManifests.Editing;

/// <summary>One function the expression language offers, with the snippet the picker inserts for it.</summary>
public sealed record ExpressionFunction(string Name, string Signature, string Description, string Snippet, int CaretFromEnd = 1);
