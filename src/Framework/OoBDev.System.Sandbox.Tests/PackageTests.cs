using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>Signed packages (REQ-006): a package is accepted only when signed by a trusted key and unchanged.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class PackageTests
{
    private static readonly PluginManifest _template = new(
        "telemetry",
        "1.2.0",
        "1.0",
        new Dictionary<string, string> { ["win-x64"] = "win-x64/telemetry.exe" },
        new PluginPermissions([new FilePermission("data", "rw")], [new NetworkPermission("api.example.org", 443)]),
        new Dictionary<string, string>());

    private readonly ITempFileFactory _temp = new TempFileFactory(new ServiceCollection().BuildServiceProvider());
    private readonly ECDsa _publisher = ECDsa.Create(ECCurve.NamedCurves.nistP256);

    private PackageVerifier Verifier(string keyId = "publisher-1")
    {
        var trust = new TrustList();
        trust.Trust(keyId, _publisher.ExportSubjectPublicKeyInfo());
        return new PackageVerifier(trust);
    }

    private string Build(ITempDirectory directory)
    {
        Directory.CreateDirectory(Path.Combine(directory.DirectoryPath, "win-x64"));
        File.WriteAllText(Path.Combine(directory.DirectoryPath, "win-x64", "telemetry.exe"), "binary");
        File.WriteAllText(Path.Combine(directory.DirectoryPath, "readme.txt"), "docs");
        PackageSigner.Sign(directory.DirectoryPath, _template, _publisher, "publisher-1");
        return directory.DirectoryPath;
    }

    [TestMethod]
    public void Verify_SignedUnchangedPackage_IsAccepted()
    {
        using var directory = _temp.GetTempDirectory();
        var path = Build(directory);

        var result = Verifier().Verify(path);

        Assert.IsTrue(result.Accepted, result.Detail);
        Assert.AreEqual("telemetry", result.Manifest!.Id);
        Assert.HasCount(2, result.Manifest.Files);
    }

    [TestMethod]
    public void Verify_NoSignatureFile_IsRefused()
    {
        using var directory = _temp.GetTempDirectory();
        var path = Build(directory);
        File.Delete(Path.Combine(path, PackageSigner.SignatureFile));

        Assert.AreEqual(PackageRefusal.MissingSignature, Verifier().Verify(path).Refusal);
    }

    [TestMethod]
    public void Verify_UntrustedKey_IsRefused()
    {
        using var directory = _temp.GetTempDirectory();
        var path = Build(directory);

        Assert.AreEqual(PackageRefusal.UnknownKey, Verifier("someone-else").Verify(path).Refusal);
    }

    [TestMethod]
    public void Verify_RevokedKey_IsRefused()
    {
        using var directory = _temp.GetTempDirectory();
        var path = Build(directory);
        var trust = new TrustList();
        trust.Trust("publisher-1", _publisher.ExportSubjectPublicKeyInfo());
        trust.Revoke("publisher-1");

        Assert.AreEqual(PackageRefusal.UnknownKey, new PackageVerifier(trust).Verify(path).Refusal);
    }

    [TestMethod]
    public void Verify_ManifestEditedAfterSigning_IsRefusedAsBadSignature()
    {
        using var directory = _temp.GetTempDirectory();
        var path = Build(directory);
        var manifest = Path.Combine(path, PackageSigner.ManifestFile);
        File.WriteAllText(manifest, File.ReadAllText(manifest).Replace("\"data\"", "\"C:/\"", StringComparison.Ordinal));

        Assert.AreEqual(PackageRefusal.BadSignature, Verifier().Verify(path).Refusal);
    }

    [TestMethod]
    public void Verify_SignedByAnotherKeyUnderTheTrustedId_IsRefusedAsBadSignature()
    {
        using var directory = _temp.GetTempDirectory();
        var path = Build(directory);
        using var attacker = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        PackageSigner.Sign(path, _template, attacker, "publisher-1");

        Assert.AreEqual(PackageRefusal.BadSignature, Verifier().Verify(path).Refusal);
    }

    [TestMethod]
    public void Verify_ExecutableReplaced_IsRefusedAsModified()
    {
        using var directory = _temp.GetTempDirectory();
        var path = Build(directory);
        File.WriteAllText(Path.Combine(path, "win-x64", "telemetry.exe"), "malware");

        var result = Verifier().Verify(path);

        Assert.AreEqual(PackageRefusal.FileModified, result.Refusal);
        Assert.AreEqual("win-x64/telemetry.exe", result.Detail);
    }

    [TestMethod]
    public void Verify_FileAdded_IsRefusedAsUnlisted()
    {
        using var directory = _temp.GetTempDirectory();
        var path = Build(directory);
        File.WriteAllText(Path.Combine(path, "extra.dll"), "injected");

        Assert.AreEqual(PackageRefusal.UnlistedFile, Verifier().Verify(path).Refusal);
    }

    [TestMethod]
    public void Verify_FileRemoved_IsRefusedAsMissing()
    {
        using var directory = _temp.GetTempDirectory();
        var path = Build(directory);
        File.Delete(Path.Combine(path, "readme.txt"));

        Assert.AreEqual(PackageRefusal.FileMissing, Verifier().Verify(path).Refusal);
    }

    [TestMethod]
    public void Verify_ListedPathLeavesThePackage_IsRefused()
    {
        using var directory = _temp.GetTempDirectory();
        var path = Build(directory);
        var evil = _template with { Files = new Dictionary<string, string> { ["../outside.txt"] = "sha256:00" } };
        var bytes = global::System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(evil, PluginManifest.Json);
        File.WriteAllBytes(Path.Combine(path, PackageSigner.ManifestFile), bytes);
        var signature = new { keyId = "publisher-1", value = Convert.ToBase64String(_publisher.SignData(bytes, HashAlgorithmName.SHA256)) };
        File.WriteAllText(Path.Combine(path, PackageSigner.SignatureFile), global::System.Text.Json.JsonSerializer.Serialize(signature));

        Assert.AreEqual(PackageRefusal.BadPath, Verifier().Verify(path).Refusal);
    }

    [TestMethod]
    public void Verify_GarbageManifest_IsRefusedAsBad()
    {
        using var directory = _temp.GetTempDirectory();
        var path = Build(directory);
        File.WriteAllText(Path.Combine(path, PackageSigner.ManifestFile), "not json");

        Assert.AreEqual(PackageRefusal.BadManifest, Verifier().Verify(path).Refusal);
    }
}
