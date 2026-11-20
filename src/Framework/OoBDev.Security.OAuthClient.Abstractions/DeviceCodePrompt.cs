using System;

namespace OoBDev.Security.OAuthClient;

/// <summary>What a person needs to approve a device sign-in (RFC 8628).</summary>
/// <param name="UserCode">The short code to type at the verification address.</param>
/// <param name="VerificationUri">The address to open on another device.</param>
/// <param name="VerificationUriComplete">The address with the code included, when the provider offers it.</param>
/// <param name="ExpiresIn">How long the code stays valid.</param>
public sealed record DeviceCodePrompt(string UserCode, Uri VerificationUri, Uri? VerificationUriComplete, TimeSpan ExpiresIn);
