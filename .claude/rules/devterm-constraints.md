---
paths:
  - "src/Applications/DevTerm/OoBDev.DevTerm*/**"
  - "src/Extensions/OoBDev.Scpi*/**"
  - "src/**/OoBDev.System.IO.*/**"
  - "src/**/OoBDev.Devices*/**"
---

# DevTerm and device I/O constraints (imported from dev-term, verified there)

Names of the original dev-term projects (`DevTerm.*`) now map to `OoBDev.DevTerm.*`, `OoBDev.System.IO.*` and `OoBDev.Scpi`. Re-verify against real hardware before reversing any of these.

- **Moq/Castle cannot mock a method taking `Span<T>`/`ReadOnlySpan<T>`** (a ref struct can't back
  a mocked generic parameter). Interface members meant to be mockable use `ReadOnlyMemory<T>` or
  `ReadOnlySequence<T>` instead, converting to `.Span` only inside the implementation.
- **`SerialPort.BaseStream.ReadAsync(Memory<byte>, CancellationToken)` does not reliably honor
  cancellation** on real hardware (verified, not theoretical) — `DevTerm.Transports.Serial` reads
  via the `SerialPort.DataReceived` event instead (a cancelable `TaskCompletionSource`, not a
  polling loop). Don't reintroduce a direct `ReadAsync`/`Task.Run`-wrapped-sync-`Read` on
  `SerialPort` without re-verifying against real hardware — both were tried and both had real
  cancellation/lifetime hazards.
- **`SerialPort` defaults `DtrEnable`/`RtsEnable` to `false`**, unlike most terminal tools (and
  pyserial) — plenty of devices stay silent without DTR asserted. dev-term defaults both to `true`.
- **`Microsoft.Extensions.Configuration.CommandLine` has no bare-boolean-flag support** — a
  standalone `--listen` is not a valid boolean `true`; it must be `--listen true`.
- **Terminal.Gui v2 is not source-compatible with v1** — namespaces are split
  (`Terminal.Gui.App`, `.Views`, `.ViewBase`, `.Input`, `.Drivers`, ... instead of one flat
  `Terminal.Gui`), and `View.KeyDown` hands out a `Key` directly rather than a `KeyEventEventArgs`
  wrapper. v1-era examples/docs don't apply. When the installed version's actual API shape is in
  doubt, check it directly (e.g. reflect over the installed package's DLL) rather than guessing.
- **HidSharp's `HidStream` has the same cancellation hazard as `SerialPort.BaseStream`** — no event
  analogous to `DataReceived`, and its `Read`/`Write` are `BeginRead`/`EndRead`-backed under the
  default `Stream.ReadAsync`, which doesn't meaningfully honor a `CancellationToken` mid-read.
  `DevTerm.Transports.Hid`'s `HidReadStream` uses a dedicated background thread (owned, joined on
  close, bounded by `ReadTimeout`) handing reports to a `Channel<byte[]>` instead — never reintroduce
  a `Task.Run`-wrapped blocking read per call here for the same reason it was rejected for serial.
- **A zero-length `Stream.Write` is invalid on Windows HID, unlike serial/TCP** — a HID report is a
  fixed, non-zero, device-defined length; writing zero bytes throws a raw Win32 `IOException`
  instead of being a no-op (verified against real hardware: an empty typed line crashed the whole
  app before this was fixed). `HidTransport.WriteAsync` no-ops on empty data; a wrong *non-zero*
  length is a real, expected device-specific framing failure and still throws — see the next point.
- **Front ends never crash, exit or close over an error once running, and all report a lost
  connection the same way (2026-09-25):**
  - A send can legitimately fail for reasons that aren't a timeout. A generic text presenter's typed
    input can't guarantee a device's framing (a HID report's exact length, for one).
  - `Session` itself closes on any read failure, peer hang-up, or failed send, and raises
    `Session.Disconnected` *before* a failed `SendAsync` rethrows. Report lost connections from that
    one handler (`ConnectionErrorMessages.ForDisconnect`), not from a send's catch block, or they
    get reported twice. A send's catch only reports when `session.State` is still `Open`.
  - Input a parser can't encode is validation, not a disconnect: encode through `TypedInput.TryEncode`,
    never a bare `IPresenterInput.Parse`. That bare call is what crashed the CLI on non-hex text under
    `--parser hex`.
  - Catch `Exception` (not just `IsConnectionFailure`) at every UI boundary, and never leave a
    `_ = SomethingAsync()` unobserved. `TuiMode.Observe`/`MainWindow.Observe` route a faulted
    fire-and-forget task to the output pane.
  - A failed *startup* connect opens the TUI/WPF disconnected rather than exiting. Only the CLI still
    exits (code 1), so scripts see it.
- **`Session`'s read loop must never await the session's own close.** `CloseAsync`/`StopAsync` awaits
  the read-loop task, so a fault detected inside `PumpAsync` hands off to `FaultAsync` via a
  non-awaited `Task.Run` — awaiting it inline deadlocks on itself. Faults also carry a *generation*
  number, so a late fault from an already-closed or reopened connection can't tear down or be reported
  against the new one.
- **Presenters and `PresenterCatalog` are transient DI registrations; resolve the catalog once per
  session.** Presenters are stateful (line buffers, the SCPI pending-query queue), and each resolved
  catalog builds its own set. Anything that needs "this session's scpi presenter" (a control panel)
  must use the catalog that session was built from, not a fresh `GetRequiredService<PresenterCatalog>()`,
  which is a different, unconnected set.
- **Terminal.Gui.Editor's `Text` throws `InvalidOperationException` ("Call from invalid thread") if
  read off the UI thread.** This is unlike `TextField`/`MenuItem` properties, which a test thread could
  read under `TuiTestRunner.RunWithLoop` without complaint. Found writing
  `TuiModeErrorHandlingTests.ReadFailure_…`: marshal the read through `app.Invoke` and wait on it.
- **`System.Xml.Serialization.XmlSerializer` cannot serialize `Dictionary<TKey,TValue>`** — it
  throws `NotSupportedException` ("implements IDictionary") at first-use reflection time, not at
  compile time. Any type meant to round-trip through both `System.Text.Json` and `XmlSerializer`
  (see `DevTerm.UiDefinitions`, `DevTerm.DeviceManifests`) uses a plain `List<T>` of a small
  Key/Value class instead of a dictionary, which both serializers handle natively with no
  special-casing.
- **A WPF `Window` created on a manually-spun-up STA thread (no `Application.Run()`) needs a real
  `DispatcherSynchronizationContext` installed and a `Dispatcher.PushFrame` loop actually running**
  — without both, `await` continuations after genuine async I/O resume on an arbitrary thread-pool
  thread instead of the STA thread that owns the UI, throwing "The calling thread cannot access
  this object because a different thread owns it." Only shows up with real I/O, not a fake
  transport whose "async" calls complete synchronously — see `DevTerm.Wpf.Tests.StaTestRunner` and
  [`docs/applications/devterm/design/testing.md`](docs/applications/devterm/design/testing.md).
- **Don't both call `MainWindow.Show()` and `MainWindow.ConnectAsync()` from the same caller** —
  `Show()` fires the real `Loaded` event, which calls `ConnectAsync` on its own; calling it again
  opens the session twice concurrently, and two concurrent readers on one `PipeReader` corrupts its
  internal state (throws "Writing is not allowed after writer was completed" from a seemingly
  unrelated later call, not from the double-open itself).
- **Terminal.Gui v2.5.0's `Application.Invoke` silently queues forever unless a real
  `Application.Run()` loop is actively pumping somewhere** — calling it from a background thread
  with no loop running, and then calling `LayoutAndDraw`/`RaiseIteration` manually, never flushes
  it. Conversely, `IInputInjector.InjectKey`/`ProcessQueue()` only reliably reaches the focused view
  when there's *no* `Application.Run()` loop running concurrently on another thread — the two don't
  compose. See `DevTerm.Console.Tests.TuiTestRunner`'s two separate run modes
  (`RunHeadless`/`RunWithLoop`) and [`docs/applications/devterm/design/testing.md`](docs/applications/devterm/design/testing.md).
- **`Session` recreates its read-loop `CancellationTokenSource` on every `OpenAsync`, not once for
  the object's lifetime** — a `CancellationTokenSource` can only ever be cancelled once, so reusing
  the same one across a Close-then-reopen would start the new read loop with an already-cancelled
  token, ending it immediately and silently (found while adding a Connect/Disconnect menu item —
  see `DevTerm.Core.Tests.SessionTests.OpenAsync_AfterClose_RestartsTheReadLoopForRealIncomingData`,
  a regression test confirmed to fail without this fix). Both `TuiMode`/`MainWindow`'s
  `ToggleConnectionAsync` methods call `Session.OpenAsync`/`CloseAsync` repeatedly on the same
  `Session` instance, so this matters for any future code doing the same.
- **`Session.OpenAsync`'s own `cancellationToken` parameter must never be forwarded into
  `Task.Run`'s scheduling-cancellation argument for the read loop** — a mechanical CA2016/MA0040
  "forward the CancellationToken parameter" fix did exactly that
  (`Task.Run(() => PumpAsync(_readLoopCts.Token), cancellationToken)`) and it's a real, silent-failure
  bug: if `OpenAsync`'s token were cancelled before the thread pool started the delegate, `Task.Run`
  yields an already-`Canceled` task **without ever running `PumpAsync`** — the read loop never starts,
  but `OpenAsync` itself doesn't throw (it doesn't await `_readLoopTask`), so the connection reports
  open with no data ever read; a later `StopAsync`'s `await _readLoopTask` then throws
  `TaskCanceledException` uncaught out of `CloseAsync`/`DisposeAsync`. The read loop's lifetime is
  governed solely by `_readLoopCts.Token` (owned by `StopAsync`), which is a different token than the
  one `OpenAsync` receives — pass `CancellationToken.None` explicitly to `Task.Run` to satisfy
  CA2016 without misusing a token for a lifetime it doesn't own. Found via a pre-merge branch review,
  not a live symptom — worth rechecking after *any* future "forward the CancellationToken parameter"
  cleanup pass near `Task.Run`/`Task.Factory.StartNew`, since the analyzer can't tell the two tokens
  apart.
- **`Task.Run(Func<Task>)`'s returned `Task`'s `Id` is not the `Id` of the async method actually
  executing** — `Task.Run` schedules an outer "runner," then unwraps it into a separate proxy `Task`
  (the one you get back and store) that completes when the inner async state machine's own `Task`
  does; the two are different `Task` objects with different `Id`s. A `Session.StopAsync` reentrancy
  check that compared `Task.CurrentId == _readLoopTask.Id` (`_readLoopTask` being the field set from
  `Task.Run(() => PumpAsync(...))`) never matched even when genuinely called back from inside
  `PumpAsync`'s own call stack (a synchronous `Output` handler invoking `SendAsync`), so the deadlock
  it was meant to catch reproduced identically. Fixed with an `AsyncLocal<bool>` set at the top of
  `PumpAsync` instead — see the next point and
  [039](docs/bugs/resolved/039-sendasync-from-read-loop-deadlock.md).
- **An `AsyncLocal<T>` set inside a method stays visible to everything that method calls — sync or
  async — until that method returns, but never to a separate, concurrent caller on the same object**,
  which is exactly the distinction `Session` needs to detect "`SendAsync` was called synchronously
  from within the read loop's own `Output` handler" (must fail fast — awaiting the read loop from
  there deadlocks on itself) without misfiring on an unrelated, legitimately concurrent `CloseAsync`
  while the read loop is merely still blocked in a read (must await normally). `PumpAsync` sets its
  instance's `_onReadLoop.Value = true` at entry; `StopAsync` checks it before awaiting the read-loop
  task. The one exception that must *not* inherit `true`: `PumpAsync`'s own tail `_ = Task.Run(() =>
  FaultAsync(...))` for a naturally-ended loop (peer closed, read failed) is a deliberate
  fire-and-forget to a *different* logical chain, not reentrancy, and needs `StopAsync` to await it
  normally — wrap that specific `Task.Run` call in `using (ExecutionContext.SuppressFlow())` or the
  spawned task inherits the ambient `true` and false-positives.
- **A `PipeReader.ReadAsync` in a test blocks forever against a genuinely, correctly empty reply** —
  it has no signal for "confirmed nothing's coming," only "no data yet": a writer that flushes zero
  bytes without ever calling `Advance` or completing the pipe never unblocks a pending `ReadAsync`.
  This made `DevTerm.Transports.Usbtmc.Tests.UsbtmcTransportTests`'s
  `WriteAsync_TwoConsecutiveValidZeroLengthMessages...` test hang (not fail — hang) against a
  legitimately well-formed, zero-length USBTMC reply, even though the production `ReadReply` logic
  handling it was correct. `PipeReader.TryRead(out ReadResult)` is the non-blocking alternative,
  returning `false` immediately when nothing's been written and the pipe is still open — use it (not
  a bare `ReadAsync`) whenever a test's whole point is asserting that a read produced nothing.
- **A real-hardware serial/transport test choosing the terminatorless `RawPresenter` over
  `AsciiPresenter` must match the actual device, not just avoid an `AsciiPresenter` hang** —
  `RawPresenter` emits whatever bytes arrived in a single transport read verbatim, firing a separate
  `Session.Output` per read with no buffering. For a device with a real line terminator, a reply can
  legitimately arrive over the wire in more than one read; `RawPresenter` then silently surfaces only
  the first fragment (e.g. `Received: H` instead of the real `*IDN?` string) as if it were the whole
  reply — the test still passes (non-empty, no fault/timeout, which is all these tests assert) even
  though the captured value is garbage. Confirmed against a real HP 34401A
  (`docs/test/2026-09-25-18-57-22.md`; `tests/DevTerm.Console.Tests/RealHardwareSerialTests.cs`).
  `RawPresenter` is only trustworthy for a genuinely terminatorless device (the Korad KA3005P/
  KA6003P, the Rigol DS1102E) — a device with a real terminator (the HP 34401A's LF, both Tektronix
  scopes) needs `AsciiPresenter` instead, which buffers until that terminator even though it's
  tempting to reach for `RawPresenter` everywhere just to sidestep `AsciiPresenter` never flushing a
  terminatorless reply.
- **An editable WPF `ComboBox`'s `IsKeyboardFocused` stays `false` even right after a successful
  `Focus()` call** — WPF delegates actual keyboard focus to the control's internal text-box part
  (`IsEditable="True"`), so the `ComboBox` wrapper itself never reports keyboard focus; check
  `IsKeyboardFocusWithin` instead when testing whether focus is "in" an editable `ComboBox`. Found
  while writing a regression test for bug 062 (`MainWindow.SendBox`, used for command history
  recall): both new tests failed against `IsKeyboardFocused` despite the production fix (restoring
  focus to `SendBox` after a mouse-driven send) being correct — see
  `docs/bugs/resolved/062-sendbox-arrow-keys-lose-focus-after-send.md`.
- **A WPF `{Binding ...}` doesn't populate a control synchronously from a constructor-assigned
  `DataContext` if the window is never `Show()`n** — checked directly: a `TextBox` bound to a
  view-model property that already had a value at construction time still read back empty
  immediately afterward, in a test that (deliberately, per the convention below) never calls
  `Show()`. A single `StaTestRunner.DoEvents()` pump after construction is enough; property-change-
  driven updates *after* that initial pump apply synchronously as normal. See
  `DevTerm.Wpf.Tests.DeviceProfilesWindowTests`.
- **`Window.DialogResult` throws `InvalidOperationException` unless the window was actually shown
  via `ShowDialog()`** — relevant because `DeviceProfilesWindow` sets it when its shared
  `ConnectionEditorViewModel.CloseRequested` fires, and tests drive that view model directly without
  ever calling `ShowDialog()` (the same "don't `Show()` a window under direct test" convention
  below, just hitting WPF's dialog-specific version of it). The window catches and ignores that
  specific exception there — `ConnectionEditorViewModel.Result` is already set correctly regardless
  of whether `DialogResult` could be set.
- **A `MenuItem`'s `Key`/`InputGestureText` argument (Terminal.Gui) or `InputGestureText` (WPF)
  only labels a keyboard shortcut for display — neither registers a live accelerator by itself.**
  `TuiMode`'s Ctrl+Q previously did nothing despite being advertised in the title bar; the real fix
  needed an explicit handler on the global `Application.KeyDown` event (a per-view `Window.KeyDown`
  handler doesn't reliably see a key already routed to a focused child first — the send field
  normally has focus). `MainWindow`'s WPF menu needed the equivalent: an explicit `PreviewKeyDown`
  check, not just `MenuItem.InputGestureText`.
- **Terminal.Gui v2.5.0's generic `OptionSelector<T>.Values` cannot be set directly** — it throws
  `InvalidOperationException` ("Setting Values directly is not allowed"); that selector derives its
  values from `Enum.GetValues<T>()`, and `T` must be `struct, Enum`. dev-term's forms instead use the
  non-generic `OptionSelector` with its `Labels` list (`FormRenderer`), wrapped in a `TuiChoice` so a host or test
  reads and sets a choice by option text.
- **Terminal.Gui's headless "dotnet" driver reports `fg=(255,255,255) bg=(255,255,255)`
  (white-on-white, invisible) for any cell still on the default, unstyled color scheme** — there's
  no real terminal behind headless mode to resolve an actual theme's colors. Confirmed by reflecting
  `Cell.Attribute` (a real `Terminal.Gui.Drawing.Attribute` with true RGB `Foreground`/`Background`,
  not just a 16-color name) for a plain `Label` and the window border, both white-on-white, while a
  menu bar highlight or a focused field reported real, distinguishable colors. Only relevant once
  you try to render the buffer as an actual image (`DevTerm.Console.Tests.TuiScreenshot`) — a plain
  `Cell.Grapheme` text dump (`TuiTestRunner.DumpBuffer`) never touches color and was unaffected. Any
  future color-aware TUI capture should fall back to a sane default (black-on-white) when a cell's
  foreground equals its background, rather than trusting the driver's reported color literally.
- **A WPF `Window` that's only ever `Measure`d/`Arrange`d, never `Show()`n, renders as a blank image
  under `RenderTargetBitmap`** — confirmed by inspecting the actual output pixels, not assumed. It
  needs a real (if off-screen/invisible) `Show()` to get a `PresentationSource`/compositor target
  before `RenderTargetBitmap.Render()` produces real content. See `DevTerm.Wpf.Tests.WpfScreenshot`.
- **`MainWindow.OnClosing`'s cancel-then-async-cleanup-then-reclose pattern must `await
  Dispatcher.Yield()` before its second `Close()`.** Awaiting already-completed tasks (a session that
  never connected, an instantly-closing transport) continues *synchronously inside the first
  `Close()`*, so the second one threw "Cannot ... Close ... while a Window is closing" — a real crash
  closing the app, first mistaken for a test-automation quirk. Fixed and covered by
  `MainWindowTests.Close_*`, so closing a shown `MainWindow` in a test is fine now.
- **A plain `View` has `CanFocus = false`, and an unfocusable container blocks focus for all its
  children** — the Connection Editor's scrollable form container silently made every field
  un-typeable (no focus highlight, Tab did nothing) from the commit that introduced it until it was
  set to `CanFocus = true`. No headless test caught it because none asked whether a field could hold
  focus; `ConfigureModeTests.FormFields_CanTakeFocus_*` does now. Related: a scrolled container does
  not scroll a newly focused child into view by itself — `ConfigureMode` reveals each direct child on
  `HasFocusChanged` (`HasFocusEventArgs.NewValue`).
- **Real-console TUI driving works and is the way to check input bugs headless tests can't**: launch
  `conhost.exe <DevTerm.Console.exe> <args>` (force the editor with `--transport hid --vendorid 0
  --productid 0`), find its `ConsoleWindowClass` window, inject key events with
  `AttachConsole` + `WriteConsoleInput` to the `CONIN$` handle, and screenshot with `PrintWindow`.
  Use a key the app handles globally (PageUp/PageDown scroll the editor form) as a control to prove
  the injection itself is reaching the app before drawing conclusions from "nothing happened".
- **Real-WPF-app driving past a blocking `MessageBox.Show()` works the same way, for the same
  reason** — `MainWindow.ConnectAsync`/`ToggleConnectionAsync`/`SwitchProfileAsync`'s failure paths
  are undertested by design (`MessageBoxShow` is a real modal with no automated way to dismiss it;
  see `MainWindowSwitchProfileTests`'s doc comment) but still checkable live: launch
  `DevTerm.Wpf.exe` with args pointed at a connection that will fail, enumerate its windows via
  `user32.dll EnumWindows`/`GetWindowText` to find the resulting dialog and `EnumChildWindows` for
  its OK button, then drive the button with `SendMessage(hwnd, BM_CLICK=0x00F5, 0, 0)` — this
  bypasses the button click entirely rather than needing real keyboard/mouse input, which matters
  because `SetForegroundWindow`/`SendKeys` from an unrelated (non-interactive) process reliably
  fail to focus the dialog under Windows' foreground-lock rules. Confirmed a reported "WPF crashes
  on connection error" did not reproduce this way (process exited cleanly both times, no `.NET
  Runtime`/`Application Error` event log entries) — see docs/changes/2026-09-22.md.
- **The Terminal.Gui headless key injector (`IInputInjector`/`TuiTestRunner.TypeText`) degrades
  after enough `Application.Init`/`Shutdown` cycles have already run in the same test process** —
  this was already known for a single class (see the `Application.Invoke`/`IInputInjector` note
  above and `TuiModeTests.CtrlQ_RequestsStop`'s doc comment), but turned out to be cross-class too: a
  new screenshot test using `TypeText` left a *later, different* test's own `TypeText` call unable
  to reach the focused field, even though each test's own `Application.Init`/`Shutdown` cycle
  completed cleanly. Injecting *mouse* events (`InjectMouse`) counts too: a real-mouse double-click test made
  `TuiModeTests.TypingAndEnter…` fail in the full run (it passed alone) — a one-off probe is fine,
  a permanent test isn't. When a test doesn't need to prove real key routing — e.g. a screenshot just
  wants a field showing some text — set the control's `.Text` directly and call
  `Application.LayoutAndDraw(true)` instead of injecting keys.
- **The .NET config binder appends bound array items to a non-empty default array, and silently
  drops a scalar bound to an array property.** `CliOptions.Presenter` is a `string[]` (a profile lists
  several display presenters), verified empirically: a default of `["hex"]` plus a bound `["a"]` came
  out `hex|a`, so the property defaults to `[]` and `CliOptions.EffectivePresenters` applies the
  "hex" fallback instead. And an old-format profile (`"Presenter": "hex"`), `--presenter ascii,hex`,
  and `DEVTERM_PRESENTER` all arrive as a scalar the binder ignores, so every call site binds through
  `DevTermConfiguration.Bind(configuration, options)` (never a bare `configuration.Bind`), which
  splits a scalar `Presenter` on commas afterwards. `Parser` (which presenter encodes typed lines)
  is separate and null on old profiles — `EffectiveParser` falls back to the first presenter.
- **WPF tests must not run in parallel: WPF's XAML parser races across STA threads.** Two windows
  parsed at the same moment on different threads (each `StaTestRunner.Run` is its own STA thread)
  intermittently failed with `XamlParseException: The given key 'Title' was not present in the
  dictionary` (from a `ConcurrentDictionary` inside the parser) or a bare `NullReferenceException`, about
  once every few full-suite runs. `DevTerm.Wpf.Tests` is now `[assembly: DoNotParallelize]`, so a new test
  class can't bring it back by forgetting the attribute. Related: `StaTestRunner` used to rethrow with a
  bare `throw exception;`, which reported every failure at its own line; it now uses `ExceptionDispatchInfo`.
- **A WPF screenshot taken right after a UI change that ran synchronously shows the *previous* state.**
  `RenderTargetBitmap` renders whatever the last layout pass produced. If the code under test updated the
  UI synchronously (e.g. `ToggleConnectionAsync` against a `FakeTransport`), a "wait until X" poll that is
  already true pumps nothing. `wpf-main-window.png` showed a connected window under a "disconnected"
  caption because of this. Call `StaTestRunner.DoEvents()` plus `window.UpdateLayout()` before
  `WpfScreenshot.Save`, and assert the state you mean to capture.
- **After changing a Terminal.Gui view's `Y`/`Visible`, its `Frame` is stale until the next layout
  pass.** A reveal-on-focus that read `Frame.Y` right after a control-panel section collapse scrolled to the
  old, expanded position. Track the positions you assigned yourself (`ControlPanelMode`'s `PanelState.Tops`)
  instead of reading `Frame` right after mutating layout. Separately, `View.SetFocus()` in a headless
  `RunHeadlessApp` test *does* raise `HasFocusChanged`: enough to test focus-driven UI like the panel's
  `Sends:` footer, with no key injection, so it avoids the injector-degradation problem above.
- **In a WPF `Grid` star column, a fixed-`Width` control with the default `HorizontalAlignment`
  (Stretch) is centered, not left-aligned.** A `Width=160` `ComboBox` sat in the middle of the Busylight
  panel's Track row until the row content got `HorizontalAlignment = Left`.
- **WPF decodes a truncated PNG without error.** `BitmapDecoder.Create(..., BitmapCacheOption.OnLoad)`
  accepted the first 200 bytes of a ~10 KB PNG and returned a frame, so a successful decode says nothing
  about completeness. The Stream Monitor finds a capture's end structurally (`StreamContentEndFinder`)
  instead of "does it decode yet".
- **Terminal.Gui `Label.Text` treats `_` as a hotkey marker.** `Rigol_DG1062Z_…` rendered as
  `RigolDG1062Z_…`. Any label showing data (device names, file names) needs
  `HotKeySpecifier = (Rune)0xFFFF`.
- **A WPF `ComboBox` using `DisplayMemberPath` auto-generates an untrimmed `TextBlock` per item,
  which triggers a real hard layout clip (`LayoutInformation.GetLayoutClip`) once a real device's
  description is long enough to overflow the field** — unlike `TextTrimming`, which only shrinks a
  `TextBlock`'s own `DesiredSize` when set explicitly. Found via `UiLayoutReviewTests`'
  `DeviceProfilesWindow_EachTransport` against a real attached Prolific USB-to-serial adapter at
  COM3, whose real, long description overflowed `DeviceProfilesWindow`'s detected-ports picker.
  Fixed by replacing `DisplayMemberPath` with an explicit `ItemTemplate` (built via
  `FrameworkElementFactory`) giving the generated `TextBlock` `TextTrimming.CharacterEllipsis` plus
  a `ToolTip` bound to the same text — matching the pattern already used by
  `StreamMonitorWindow.xaml`'s `Subtitle` TextBlock. `DisplayMemberPath` and `ItemTemplate` can't
  both be set on the same `ItemsControl` (WPF throws), so this is an either/or choice, not additive.
- **A Terminal.Gui `Label` wraps a line longer than its width, and whatever falls past its `Height` is
  silently dropped.** A two-line label whose first line was too long lost its second line entirely. Keep
  each explicit line shorter than the label's width.
- **A sibling view positioned at a fixed `Y` below a `Label` that can wrap breaks once the label's
  content grows past one line.** `PlaybackMode`'s "available presenters" label (`Width = Dim.Fill()`,
  no explicit `Height`, so it auto-sizes to however many wrapped lines its text needs) had the output
  `Editor` below it pinned to a literal `Y = 3` — fine while the presenter list fit on one line, but
  adding more device presenters (radexone/zoomh4n/de5000) made the label wrap to two lines, and the
  Editor then overlapped its second line, both clipping the wrapped text and corrupting the Editor's
  own top row (`TuiToolWindowLayoutTests.Playback_PartWayThroughWithANote (80,25)`, caught by the
  `TuiReview`/`TuiLayoutAssert` overlap check). Fixed by positioning the Editor at
  `Pos.Bottom(availableLabel)` instead of a literal offset — any view following a wrapping label needs
  a relative `Pos`, not a number baked in for whatever line count happened to be true at the time.
- **A headless Terminal.Gui resize works:** `app.Driver.SetScreenSize(w, h)` followed by
  `LayoutAndDraw(true)` on v2.5.0's "dotnet" driver acts as a real resize and fires `SubViewsLaidOut` with the
  new viewport width. Calling `SetContentSize` from a `SubViewsLaidOut` handler settles without looping, as
  long as it only runs when the value actually changed; that's how the control panel's form sizes its
  sideways scroll area.
- **A library's `<None Include="Manifests\**\*" Link="manifests\%(RecursiveDir)..."
  CopyToOutputDirectory="PreserveNewest">` is copied into the output of every app and test project that
  references it, even indirectly.** That's how the bundled device manifests reach both apps and the tests
  without per-project wiring.
- **The WPF project's implicit usings don't include `System.IO`.** `IOException`/`InvalidDataException`
  fail with CS0103 there without an explicit `using System.IO;`, unlike the console and library projects.
- **WPF `Window.Owner` can't be set to a window that has never been shown.** It throws "Cannot set Owner
  property to a Window that has not been shown previously", which is exactly a `MainWindow` under test
  (constructed, never shown). Set `Owner` only right before a real `Show()`/`ShowDialog()`.
- **A Terminal.Gui `Button` with `ShadowStyles.None` still takes two rows, and `Height = 1` makes it disappear
  entirely.** Lay out button rows from a fixed anchor (`Pos.Bottom(label) + 1`), not `Pos.Bottom(button)`.
- **Terminal.Gui `Window.Disposing` doesn't fire when the app shuts down after `Run` returns**, including in
  headless tests. Do end-of-run cleanup (closing a log, say) explicitly after `app.Run(...)`, not in a
  `Disposing` handler.
- **A redirected Windows console writes stdout in the OEM code page:** `—` becomes `-` and `·` becomes `?` in
  captured CLI output. Keep CLI-facing text to ASCII punctuation, or it arrives mangled in scripts and
  transcripts.
- **`DateTimeOffset.AddSeconds(1.2)` lands one tick short and prints as `00:01.199`.** Use
  `AddMilliseconds` for exact timestamps in tests.
- **WPF Fluent `ThemeMode` is still `[Experimental("WPF0001")]` on .NET 10.** Any use fails the build under
  TreatWarningsAsErrors. It also re-templates every control with much larger metrics, and Dark's window
  background is transparent (Mica), so a `RenderTargetBitmap` capture comes out white on white. dev-term themes
  the stock templates itself (`WpfTheme`) instead.
- **The stock WPF control chrome is hard-coded light.** Overriding `SystemColors` keys only fixes controls whose
  styles read them (TextBox, menus, status bar). Button hover/pressed, the ComboBox toggle and editable box, the
  MenuItem drop-down popup (`#F0F0F0`) and the ListBox background stayed light under light dark-theme text, and
  needed replacement templates (`DarkControls.xaml`).
- **Terminal.Gui v2.5.0's built-in "Dark"/"Light" themes don't set a background.** `Base` stays `None` (the
  terminal's own), so each is unreadable on the opposite-colored terminal.
- **`SchemeManager.AddScheme` overrides are process-wide, not per `IApplication`.** They survive
  `Application.Init` and later `ThemeManager.Theme` switches. A test that applies a theme must call
  `TuiTheme.Restore()` and `ActiveTheme.Reset()`, and run `[DoNotParallelize]`.
- **A static-event subscription made in `TuiMode.BuildWindow` outlives the test that built the window.**
  Headless tests never dispose their windows, so `Disposing` never fires, and a later `ActiveTheme.Select` from
  another test's thread hit the stale handler: "Call from invalid thread". Marshal with `app.Invoke` when not on
  the UI thread, and unsubscribe once `app.Driver is null`.
- **An XSHD highlighting definition's colors are fixed once loaded.** A live theme switch has to swap
  `Editor.HighlightingDefinition` for a new definition (`OutputHighlighting.For(theme)`, cached per theme).
- **Windows PowerShell 5's `Get-Content`/`Set-Content` corrupt non-ASCII characters in UTF-8 source** ("●" came
  back as three garbage characters) and add a BOM and CRLF. Use `[IO.File]::ReadAllText`/`WriteAllText` with
  `UTF8Encoding($false)`, or the Edit tool.
- **`XmlSerializer` turns a null list into an empty one on the round trip.** A null `List<T>` property (e.g.
  `ButtonControl.ParameterFieldIds`) comes back as `[]`, so JSON→XML→JSON isn't byte-identical even though
  JSON→JSON is. Compare field by field in XML round-trip tests.
- **A Terminal.Gui `Window` embeds as an ordinary subview.** The manifest editor adds
  `ControlPanelMode.BuildWindow`'s window to a `FrameView` for its live preview; it lays out and takes input.
- **`app.End(token)` doesn't dispose a Terminal.Gui window, and `View.Dispose()` raises `Disposing` on every
  call** (twice → twice, no exception). Cleanup hooked on `Disposing` needs an explicit `Dispose()` after a
  nested `Run`, and must tolerate running twice.
- **`ShadowStyle = ShadowStyles.None` still leaves the shadow's column,** so buttons placed at
  `Pos.Right(prev) + 1` show a two-column gap. Drop the `+ 1` to pack shadowless buttons.
- **Private constants need the `_` prefix** (`private const string _buttonKind`), or the build fails with
  IDE1006.
- **A `JsonStringEnumConverter<T>` on the enum type writes names but still reads numbers**, so adding one to an
  existing enum stays backward-compatible (`FormDefinitionGeneratorTests.ChoiceStyle_IsWrittenByName_AndStillReadsAsANumber`).
- **A window-local `ItemContainerStyle` without `BasedOn` replaces the implicit theme `ListBoxItem` style.**
  The Device Profiles list kept the stock light selection frame under Dark because of its EventSetter style.
  `BasedOn` a type key doesn't help, since `DarkControls` is merged after `InitializeComponent`. Use
  `ListBox.MouseDoubleClick` plus `ItemsControl.ContainerFromElement` instead.
- **Closing a dirty `DeviceProfilesWindow` in a test pops a real modal `MessageBox` and hangs the run.** Set
  `ViewModel.ConfirmDiscardChanges = () => true` first. If a run does hang, use `--blame-hang-timeout`
  rather than killing every `testhost` (that kills other runs on the machine too).
- **A WPF menu popup's content has an animated opacity right after `IsSubmenuOpen = true`.** Reading it then
  made contrast look like 1.16:1 though the capture was fine. Don't fold the popup root's opacity into color
  checks.
- **The stock Aero2 GroupBox draws a hard-coded white inner border, and the stock ListBoxItem draws a
  `#DADADA` frame round an unfocused selected row.** Neither follows `SystemColors` overrides; both need
  dark templates (`DarkControls.xaml`).
- **A nested Terminal.Gui `app.Run` started inside an `Application.Invoke` callback never drains later
  `Invoke`s**, so a test waiting on one hangs. The nested loop does keep firing `AddTimeout` timers, which is
  how `TuiReview.Modal` opens, inspects and closes real dialogs.
- **Enter in a Terminal.Gui `TextField` presses the default button only via a real key**
  (`app.Keyboard.RaiseKeyDownEvent(Key.Enter)`). `InvokeCommand(Command.Accept)` on the field reports
  unhandled and doesn't redirect, so it's no test of default-button behavior (it misled a 2026-09-25 probe
  into documenting "Enter doesn't connect").
- **After `app.Begin(window)`, `app.TopRunnableView == window`, and during a nested `Run` it's the dialog.**
  That's how one global KeyDown handler tells which window is on top (`ConfigureMode.OwnsQuitKey`).
- **A shadowless Terminal.Gui `Button` still has a transparent 1-row bottom margin** (its frame is 2 rows). A
  shadowed button's shadow is drawn on the next row, over whatever sibling is there, which is why panel
  buttons are shadowless.
- **Terminal.Gui.Editor has `WordWrap` (soft wrap).** Without it, setting `CaretOffset` to the end scrolls
  the pane sideways to the last line's end, hiding every line's start.
- **WPF's dashed focus-visual rectangle only renders when the keyboard was the most recently used
  input device** (`KeyboardDevice.IsKeyboardMostRecentInputDevice()`, confirmed as a real symbol in
  `PresentationFramework.dll` alongside `ShowFocusVisual`/`AlwaysShowFocusVisual`) — it tracks real
  `InputManager` keyboard activity, not a programmatic `Keyboard.Focus(element)` call with no actual
  key ever pressed in-process. A headless test that calls `Keyboard.Focus` directly (no real window
  message pump) can assert `element.IsKeyboardFocused` and screenshot everything else about the
  focused state, but will never see the dashed rectangle itself — that's a test-environment gap, not
  evidence the focus visual is missing from the app. Found reviewing the Manifest Editor's
  keyboard-focus-visual behavior at 2026-09-30: `DarkControls.xaml`'s custom `DevTerm.FocusVisual`
  style (dark theme) and the stock Aero2 default (light theme) are both correctly wired to `Button`
  per the code — confirmed by reading it, not by a screenshot that could never show it either way.
- Verify against real hardware before trusting a fix, when hardware is available — several bugs in
  this codebase (all of the above) were only caught by testing against actual devices, not by unit
  tests alone. `docs/changes/` records what was verified this way.
