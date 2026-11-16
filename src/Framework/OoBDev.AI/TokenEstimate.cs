namespace OoBDev.AI;

/// <summary>
/// A rough token count: one token per four characters, rounded up. It is an estimate only, not the count a model
/// or a provider bills; real counts need the model's own tokenizer or the provider's usage field.
/// </summary>
public static class TokenEstimate
{
    /// <summary>Characters per estimated token.</summary>
    public const int CharactersPerToken = 4;

    /// <summary>Estimates the tokens in <paramref name="text"/>; null or empty is zero.</summary>
    public static long Of(string? text) =>
        string.IsNullOrEmpty(text) ? 0 : (text.Length + CharactersPerToken - 1) / CharactersPerToken;
}
