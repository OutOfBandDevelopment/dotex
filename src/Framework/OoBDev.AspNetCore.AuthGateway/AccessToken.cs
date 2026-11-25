using System;

namespace OoBDev.AspNetCore.AuthGateway;

/// <summary>A bearer token and when it stops being valid.</summary>
public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
