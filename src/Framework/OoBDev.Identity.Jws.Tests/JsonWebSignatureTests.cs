using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Buffers.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace OoBDev.Identity.Jws.Tests;

[TestClass]
public class JsonWebSignatureTests
{
    // RFC 7515 appendix A.1: the header and payload keep their CRLF and spaces.
    private const string _rfcHeader = "{\"typ\":\"JWT\",\r\n \"alg\":\"HS256\"}";
    private const string _rfcPayload = "{\"iss\":\"joe\",\r\n \"exp\":1300819380,\r\n \"http://example.com/is_root\":true}";
    private const string _rfcKey = "AyM1SysPpbyDfgZld3umj1qzKObwVMkoqQ-EstJQLr_T-1qS0gZH75aKtMN3Yj0iPS4hcgUuTwjAzZr1Z9CAow";
    private const string _rfcToken = "eyJ0eXAiOiJKV1QiLA0KICJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJqb2UiLA0KICJleHAiOjEzMDA4MTkzODAsDQogImh0dHA6Ly9leGFtcGxlLmNvbS9pc19yb290Ijp0cnVlfQ.dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";

    private const string _header = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";
    private const string _payload = "{\"sub\":\"1234567890\",\"name\":\"Test\"}";

    private static byte[] Key => Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef");

    [TestMethod]
    [TestCategory("Unit")]
    public void SignHs256_Rfc7515AppendixA1_MatchesTheSpecificationToken()
    {
        // Stage
        var key = Base64Url.DecodeFromChars(_rfcKey);

        // Test
        var token = JsonWebSignature.SignHs256(_rfcHeader, _rfcPayload, key);

        // Assert
        Assert.AreEqual(_rfcToken, token);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TryVerifyHs256_Rfc7515AppendixA1_ReturnsThePayload()
    {
        // Stage
        var key = Base64Url.DecodeFromChars(_rfcKey);

        // Test
        var ok = JsonWebSignature.TryVerifyHs256(_rfcToken, key, out var payload);

        // Assert
        Assert.IsTrue(ok);
        Assert.AreEqual(_rfcPayload, payload);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SignHs256_ThenVerifiedByTheLibrary_IsAccepted()
    {
        // Stage
        var token = JsonWebSignature.SignHs256(_header, _payload, Key);
        var parameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            RequireSignedTokens = true,
            ValidAlgorithms = ["HS256"],
        };

        // Test
        new JwtSecurityTokenHandler().ValidateToken(token, parameters, out var validated);

        // Assert
        Assert.AreEqual("1234567890", ((JwtSecurityToken)validated).Subject);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SignHs256_ComparedWithTheLibrarySignature_IsByteIdentical()
    {
        // Stage
        var token = JsonWebSignature.SignHs256(_header, _payload, Key);
        var parts = token.Split('.');
        using var provider = CryptoProviderFactory.Default.CreateForSigning(new SymmetricSecurityKey(Key), "HS256");

        // Test
        var expected = provider.Sign(Encoding.ASCII.GetBytes(parts[0] + "." + parts[1]));

        // Assert
        Assert.AreEqual(Base64Url.EncodeToString(expected), parts[2]);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TryVerifyHs256_PayloadChangedAfterSigning_ReturnsFalse()
    {
        // Stage
        var parts = JsonWebSignature.SignHs256(_header, _payload, Key).Split('.');
        var other = Base64Url.EncodeToString(Encoding.UTF8.GetBytes("{\"sub\":\"admin\"}"));

        // Test
        var ok = JsonWebSignature.TryVerifyHs256(parts[0] + "." + other + "." + parts[2], Key, out var payload);

        // Assert
        Assert.IsFalse(ok);
        Assert.AreEqual(string.Empty, payload);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TryVerifyHs256_WrongKey_ReturnsFalse()
    {
        // Stage
        var token = JsonWebSignature.SignHs256(_header, _payload, Key);

        // Test
        var ok = JsonWebSignature.TryVerifyHs256(token, Encoding.UTF8.GetBytes("ffffffffffffffffffffffffffffffff"), out _);

        // Assert
        Assert.IsFalse(ok);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("{\"alg\":\"none\"}")]
    [DataRow("{\"alg\":\"RS256\"}")]
    [DataRow("{\"alg\":\"hs256\"}")]
    [DataRow("{\"typ\":\"JWT\"}")]
    [DataRow("{\"alg\":\"HS256\",\"crit\":[\"exp\"]}")]
    [DataRow("[\"HS256\"]")]
    public void TryVerifyHs256_HeaderNotPinnedToHs256_ReturnsFalseEvenWithAValidSignature(string header)
    {
        // Stage: sign the unacceptable header with the real key, as an attacker holding the key could not but a forged token tries to.
        var input = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(header)) + "." + Base64Url.EncodeToString(Encoding.UTF8.GetBytes(_payload));
        var token = input + "." + Base64Url.EncodeToString(HMACSHA256.HashData(Key, Encoding.ASCII.GetBytes(input)));

        // Test
        var ok = JsonWebSignature.TryVerifyHs256(token, Key, out _);

        // Assert
        Assert.IsFalse(ok);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TryVerifyHs256_NoneTokenWithEmptySignature_ReturnsFalse()
    {
        // Stage
        var token = Base64Url.EncodeToString(Encoding.UTF8.GetBytes("{\"alg\":\"none\"}")) + "." + Base64Url.EncodeToString(Encoding.UTF8.GetBytes(_payload)) + ".";

        // Test
        var ok = JsonWebSignature.TryVerifyHs256(token, Key, out _);

        // Assert
        Assert.IsFalse(ok);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("")]
    [DataRow("a.b")]
    [DataRow("a.b.c.d")]
    [DataRow("!!!.???.***")]
    [DataRow("eyJhbGciOiJIUzI1NiJ9.e30.")]
    public void TryVerifyHs256_MalformedToken_ReturnsFalseWithoutThrowing(string token)
    {
        // Test
        var ok = JsonWebSignature.TryVerifyHs256(token, Key, out var payload);

        // Assert
        Assert.IsFalse(ok);
        Assert.AreEqual(string.Empty, payload);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TryVerifyHs256_ShortKey_ReturnsFalse()
    {
        // Stage
        var token = JsonWebSignature.SignHs256(_header, _payload, Key);

        // Test
        var ok = JsonWebSignature.TryVerifyHs256(token, new byte[16], out _);

        // Assert
        Assert.IsFalse(ok);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SignHs256_ShortKey_Throws()
    {
        // Test and Assert
        Assert.ThrowsExactly<ArgumentException>(() => JsonWebSignature.SignHs256(_header, _payload, new byte[16]));
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("{\"alg\":\"none\"}")]
    [DataRow("{\"alg\":\"RS256\"}")]
    [DataRow("not json")]
    public void SignHs256_HeaderWithAnotherAlgorithm_Throws(string header)
    {
        // Test and Assert
        Assert.ThrowsExactly<ArgumentException>(() => JsonWebSignature.SignHs256(header, _payload, Key));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SignRs256_ThenVerified_ReturnsThePayload()
    {
        // Stage
        using var rsa = RSA.Create(2048);
        var token = JsonWebSignature.SignRs256("{\"alg\":\"RS256\"}", _payload, rsa);

        // Test
        var ok = JsonWebSignature.TryVerifyRs256(token, rsa, out var payload);

        // Assert
        Assert.IsTrue(ok);
        Assert.AreEqual(_payload, payload);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SignRs256_ThenVerifiedByTheLibrary_IsAccepted()
    {
        // Stage
        using var rsa = RSA.Create(2048);
        var token = JsonWebSignature.SignRs256("{\"alg\":\"RS256\"}", _payload, rsa);
        var parameters = new TokenValidationParameters
        {
            IssuerSigningKey = new RsaSecurityKey(rsa.ExportParameters(false)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            RequireSignedTokens = true,
            ValidAlgorithms = ["RS256"],
        };

        // Test
        new JwtSecurityTokenHandler().ValidateToken(token, parameters, out var validated);

        // Assert
        Assert.AreEqual("1234567890", ((JwtSecurityToken)validated).Subject);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TryVerifyRs256_SignedWithAnotherKey_ReturnsFalse()
    {
        // Stage
        using var signer = RSA.Create(2048);
        using var other = RSA.Create(2048);
        var token = JsonWebSignature.SignRs256("{\"alg\":\"RS256\"}", _payload, signer);

        // Test
        var ok = JsonWebSignature.TryVerifyRs256(token, other, out _);

        // Assert
        Assert.IsFalse(ok);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TryVerifyRs256_Hs256TokenSignedWithThePublicKeyBytes_ReturnsFalse()
    {
        // Stage: the algorithm-confusion attack signs HS256 with the public key as the secret.
        using var rsa = RSA.Create(2048);
        var token = JsonWebSignature.SignHs256(_header, _payload, rsa.ExportSubjectPublicKeyInfo());

        // Test
        var ok = JsonWebSignature.TryVerifyRs256(token, rsa, out _);

        // Assert
        Assert.IsFalse(ok);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SignRs256_SmallKey_Throws()
    {
        // Stage
        using var rsa = RSA.Create(1024);

        // Test and Assert
        Assert.ThrowsExactly<ArgumentException>(() => JsonWebSignature.SignRs256("{\"alg\":\"RS256\"}", _payload, rsa));
    }
}
