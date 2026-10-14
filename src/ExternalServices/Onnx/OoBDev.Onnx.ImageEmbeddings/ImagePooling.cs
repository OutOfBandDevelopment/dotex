using OoBDev.Onnx.SentenceEmbeddings;
using System;
using System.Collections.Generic;

namespace OoBDev.Onnx.ImageEmbeddings;

/// <summary>How the model output becomes one vector per image.</summary>
public enum ImagePooling
{
    /// <summary>The output is already one vector per image (<c>[batch, size]</c>), such as CLIP <c>image_embeds</c> or classifier logits.</summary>
    Output,

    /// <summary>Take the first token of <c>[batch, tokens, size]</c> hidden states (class token, as DINOv2 and ViT use).</summary>
    FirstToken,

    /// <summary>Average the tokens after the first (patch tokens) of <c>[batch, tokens, size]</c> hidden states.</summary>
    MeanPatchTokens,
}
