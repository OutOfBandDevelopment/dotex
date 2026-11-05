using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration.Tests;

internal sealed class StubInstrumentPanelProvider(params string[] profileNames) : IInstrumentPanelProvider
{
    public string Id => "stub";

    public string MenuTitle => "Stub";

    public string PickerTitle => "Stub";

    public string PresenterName => "stub";

    public string AutoDetectChoice => "Auto-detect";

    public string GenericChoice => "Generic";

    public IReadOnlyList<string> ProfileNames { get; } = profileNames;

    public bool IsAvailable(string transport, int vendorId, int productId) => true;

    public bool ConfigureForProfile(string profileName, IPresenter? presenter) => true;

    public InstrumentPanel Open(string choice, Session session, IPresenter? structuredSource) => throw new NotSupportedException();

    public string DetectProgressMessage(TimeSpan timeout) => string.Empty;

    public Task<InstrumentDetection> DetectAsync(Session session, IPresenter? structuredSource, TimeSpan timeout) => throw new NotSupportedException();
}
