using System;
using System.Collections.Generic;

namespace OoBDev.UiDefinitions;

/// <summary>
/// Optional companion to <c>IPresenter</c>, the same "optional companion" shape
/// <c>IPresenterInput</c> already establishes: a decoder that tracks named live values
/// (keyed by a <c>UiControl.Id</c>) publishes them here in addition to its normal human-readable
/// <c>IPresenter.Render</c> text, so a generic control-panel renderer can drive live
/// indicators without parsing rendered text back out.
/// </summary>
public interface IStructuredPresenter
{
    event EventHandler<IReadOnlyDictionary<string, string>>? ValuesChanged;
}
