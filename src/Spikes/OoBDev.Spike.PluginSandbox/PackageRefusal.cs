namespace OoBDev.Spike.PluginSandbox;

/// <summary>Why a package was refused (REQ-006).</summary>
public enum PackageRefusal
{
    /// <summary>Accepted.</summary>
    None,

    /// <summary>The manifest or its signature file is missing.</summary>
    MissingSignature,

    /// <summary>The manifest is not valid JSON or lacks a required field.</summary>
    BadManifest,

    /// <summary>The signing key is not in the trust list.</summary>
    UnknownKey,

    /// <summary>The signature does not match the manifest bytes.</summary>
    BadSignature,

    /// <summary>A file the manifest lists is missing.</summary>
    FileMissing,

    /// <summary>A file's SHA-256 differs from the manifest.</summary>
    FileModified,

    /// <summary>A file is in the package but not in the manifest.</summary>
    UnlistedFile,

    /// <summary>A listed path leaves the package folder.</summary>
    BadPath,
}
