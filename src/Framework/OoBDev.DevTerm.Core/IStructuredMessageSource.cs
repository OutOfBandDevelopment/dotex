using System;

namespace OoBDev.DevTerm;

/// <summary>Optional companion to <see cref="IPresenter"/> for a decoder that also emits a <see cref="StructuredMessage"/> per decoded message.</summary>
public interface IStructuredMessageSource
{
    event EventHandler<StructuredMessage>? MessageDecoded;
}
