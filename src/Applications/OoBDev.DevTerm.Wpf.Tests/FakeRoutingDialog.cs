using System;

namespace OoBDev.DevTerm.Wpf.Tests;

internal sealed class FakeRoutingDialog(Action<RoutingDialogViewModel> interact) : IRoutingDialog
{
    public void Show(RoutingDialogViewModel routing) => interact(routing);
}
