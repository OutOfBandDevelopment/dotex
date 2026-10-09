namespace OoBDev.Vision.ClipVitB32;

/// <summary>Provider key and configuration names of the in-process CLIP ViT-B/32 model.</summary>
public static class VisionGlobals
{
    /// <summary>The keyed-service key of the CLIP image embedding generator, text embedding generator and zero-shot classifier.</summary>
    public const string ClipVitB32Key = "clip-vit-base-patch32";

    /// <summary>The default configuration section of the image tower.</summary>
    public const string DefaultSection = "ClipVitB32";

    /// <summary>The default configuration section of the text tower.</summary>
    public const string DefaultTextSection = "ClipVitB32Text";
}
