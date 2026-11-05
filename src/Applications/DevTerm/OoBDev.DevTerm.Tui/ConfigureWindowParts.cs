using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Drawing;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions.Forms;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
namespace OoBDev.DevTerm.Tui;

/// <summary>The controls a test needs to drive <see cref="ConfigureMode"/> headlessly — the generated form's widgets looked up by field id, plus the hand-built parts.</summary>
internal sealed class ConfigureWindowParts
{
    /// <summary>Exposed so tests can stub <see cref="ConnectionEditorViewModel.ConfirmOverwrite"/>/<see cref="ConnectionEditorViewModel.ConfirmDiscardChanges"/> instead of hitting the real, blocking Terminal.Gui <c>MessageBox.Query</c> this class wires them to.</summary>
    public required ConnectionEditorViewModel ViewModel { get; init; }

    public required Window Window { get; init; }

    /// <summary>The scrolling container holding everything; its <c>Viewport</c> is the visible part.</summary>
    public required View FormContent { get; init; }

    /// <summary>The generated connection-field form (see <see cref="FormRenderer"/>).</summary>
    public required TuiFormParts Form { get; init; }

    public required Label ErrorLabel { get; init; }

    public required ListView ProfilesList { get; init; }

    public required Button LoadButton { get; init; }

    public required Button DeleteButton { get; init; }

    public required Button RefreshButton { get; init; }

    public required Button ExportSelectedButton { get; init; }

    public required Button ExportAllButton { get; init; }

    public required Button DeleteSelectedButton { get; init; }

    /// <summary>The transport choice — an <see cref="OptionSelector"/> of the view model's <see cref="ConnectionEditorViewModel.TransportOptions"/>; set <see cref="TuiChoice.Value"/> to pick one as a user would.</summary>
    public required TuiChoice TransportSelector { get; init; }

    public required TextField DescriptionField { get; init; }

    public required TextField PortField { get; init; }

    public required Button DetectPortButton { get; init; }

    public required TextField BaudField { get; init; }

    /// <summary>Visible when <see cref="ConnectionEditorViewModel.ConnectedDeviceNotFound"/> is true for the serial transport.</summary>
    public required Label PortNotFoundLabel { get; init; }

    public required TextField DataBitsField { get; init; }

    public required TuiChoice ParitySelector { get; init; }

    public required TuiChoice StopBitsSelector { get; init; }

    public required TuiChoice HandshakeSelector { get; init; }

    public required TextField HostField { get; init; }

    public required TextField TcpPortField { get; init; }

    public required CheckBox ListenCheckBox { get; init; }

    public required TextField VendorField { get; init; }

    public required TextField ProductField { get; init; }

    public required Button DetectHidButton { get; init; }

    public required Button DetectUsbtmcButton { get; init; }

    /// <summary>Optional - blank means "match the first device found for Vendor/Product ID" (see <see cref="ConnectionEditorViewModel.SerialNumber"/>).</summary>
    public required TextField SerialNumberField { get; init; }

    /// <summary>Visible when <see cref="ConnectionEditorViewModel.ConnectedDeviceNotFound"/> is true for the HID/USBTMC transport.</summary>
    public required Label UsbNotFoundLabel { get; init; }

    public required CheckBox IdsShowHexCheckBox { get; init; }

    /// <summary>Platform-specific BLE peripheral identifier — see <see cref="ConnectionEditorViewModel.BleDeviceId"/>.</summary>
    public required TextField BleDeviceIdField { get; init; }

    /// <summary>Runs a fresh BLE scan and offers the result via a picker — see <see cref="ConnectionEditorViewModel.SelectedBleDevice"/>.</summary>
    public required Button DetectBleButton { get; init; }

    /// <summary>Blank uses the transport's own default — see <see cref="ConnectionEditorViewModel.BleServiceUuid"/>.</summary>
    public required TextField BleServiceUuidField { get; init; }

    public required TextField BleWriteUuidField { get; init; }

    public required TextField BleNotifyUuidField { get; init; }

    /// <summary>Runs a fresh GATT enumeration of <see cref="ConnectionEditorViewModel.BleDeviceId"/> and offers the write-role pick — see <see cref="ConnectionEditorViewModel.SelectedBleWriteCharacteristic"/>.</summary>
    public required Button DetectBleCharacteristicsButton { get; init; }

    /// <summary>Offers the notify-role pick from the same already-scanned list <see cref="DetectBleCharacteristicsButton"/> populated — see <see cref="ConnectionEditorViewModel.SelectedBleNotifyCharacteristic"/>.</summary>
    public required Button PickBleNotifyCharacteristicButton { get; init; }


    /// <summary>Shown only when the loopback transport is selected — it takes no configuration.</summary>
    public required Label LoopbackInfoLabel { get; init; }

    /// <summary>One checkbox per presenter, in <see cref="ConnectionEditorViewModel.PresenterChoices"/> order — the multi-select presenter picker.</summary>
    public required IReadOnlyList<CheckBox> PresenterCheckBoxes { get; init; }

    /// <summary>Shown only when the "scpi" presenter is checked — see <see cref="ConnectionEditorViewModel.IsScpiPresenterSelected"/>. Too many profiles for one line, so a text field plus <see cref="ScpiProfilePickButton"/>.</summary>
    public required TuiChoice ScpiProfileSelector { get; init; }

    public required Button? ScpiProfilePickButton { get; init; }

    /// <summary>The send format (parser) — every presenter can encode input, so its options are <see cref="ConnectionEditorViewModel.PresenterOptions"/>.</summary>
    public required TuiChoice ParserSelector { get; init; }

    public required TuiChoice LineEndingSelector { get; init; }

    public required TextField SaveNameField { get; init; }

    public required Button SaveButton { get; init; }

    public required TextField PathField { get; init; }

    public required Button BrowseButton { get; init; }

    public required Button ImportButton { get; init; }

    public required Button ExportButton { get; init; }

    public required Button SaveAsButton { get; init; }

    public required Button ReplaceAllButton { get; init; }

    public required Button ConnectButton { get; init; }

    public required Button QuitButton { get; init; }

    /// <summary>Set once the user presses Connect (to the validated options) or Quit (to <see langword="null"/>); <see langword="null"/> until either happens.</summary>
    public CliOptions? Result { get; set; }
}
