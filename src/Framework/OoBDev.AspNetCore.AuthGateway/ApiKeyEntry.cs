namespace OoBDev.AspNetCore.AuthGateway;

/// <summary>A known API key, stored only as the hex SHA-512 hash of the key.</summary>
public sealed class ApiKeyEntry
{
    public string Id { get; set; } = string.Empty;

    public string Sha512 { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;
}
