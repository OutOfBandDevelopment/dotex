# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 (2026-11-05, UiDefinitions) Where does the Terminal.Gui form renderer live?

The Tui control example needs `FormRenderer` (form definition to Terminal.Gui views), which today sits in `src/Applications/DevTerm/OoBDev.DevTerm.Tui`. An example under `src/Examples` cannot reference an Applications project (layer rule), and the renderer has no DevTerm-specific names.

**Options**
- A (recommended): move `FormRenderer` and its tests into a new Framework library `OoBDev.UiDefinitions.Tui` (mirrors `OoBDev.UiDefinitions.Wpf`), DevTerm.Tui references it, and the Tui control example uses it. Reusable means Framework (rule `layering-and-migration-plans`); cost: one migration plan and a Terminal.Gui reference in Framework.
- B: keep it in DevTerm.Tui and skip a Tui control example. No move, but the Tui control set has no example.
- C: put the example inside DevTerm (Applications). Works today, but is not a sample for other apps.

**Default:** A, after a migration plan in `docs/design/example-ui/`; nothing moves until you confirm.

**Your answer:**

### Q2 (2026-11-05, UiDefinitions) Where does the Blazor form renderer live?

There is no Blazor renderer for the UiDefinitions language yet (only WPF and the Terminal.Gui one in DevTerm). A Blazor control example needs one, and a Razor component library takes an ASP.NET `FrameworkReference`.

**Options**
- A (recommended): new Razor class library `OoBDev.UiDefinitions.Blazor` in `src/Extensions` (optional, host-bound, depends on `OoBDev.UiDefinitions.ViewModels`), with a Blazor control example in `src/Examples` and bUnit tests. Keeps ASP.NET out of Framework (rule `layering-and-migration-plans`).
- B: put it in `src/Framework` beside `OoBDev.UiDefinitions.Wpf`. Same place as the other renderers, but breaks "Framework does not take a host framework reference".
- C: skip Blazor; the web front end uses the OpenAPI-driven path instead.

**Default:** A, after a migration plan in `docs/design/example-ui/`; nothing is created until you confirm.

**Your answer:**

### Q3 (2026-11-05, Template tools) Fold `Xslt.Cli` into the template engine?

The spike ([result](docs/design/TemplateTools/README.md)) shows `Xslt.Cli` does much more than `TemplateEngine.Cli` (11 input types, merge, sandbox) and the engine's XSLT provider cannot yet run its templates. Folding means moving `SandboxedTransformer` and the input-type mapping from `src/Tools` into a library.

**Options**
- A (recommended): fold in three steps (engine XSLT provider with extensions and navigators, CLI switches, MSTest docs run), then retire `Xslt.Cli` with a conversion note. Matches your 2026-10-30 direction; cost: the library move needs your confirmed plan.
- B: keep both tools; only document when to use which. No move, but the overlap stays.
- C: drop `TemplateEngine.Cli` and make `Xslt.Cli` the one tool (Handlebars would need a switch there).

**Default:** A, starting only with the parts that move nothing (step 1 inside `OoBDev.System`); the library move waits for your answer.

**Your answer:**

### Q4 (2026-11-05, SharedFramework) Move the proposal folders into `docs/design/`?

`Features/Proposals/` holds 134 design files in eight epic folders, outside the `docs/design/<Feature>/` convention. The [plan](docs/design/SharedFramework/migration-plan.md) moves them without changing content.

**Options**
- A (recommended): move as planned (documents only, no code), rewrite links, validate, delete `Features/`. Matches the rule that designs live in `docs/design`; cost: one large commit and link churn in the change documents.
- B: leave `Features/Proposals/` where it is and add a pointer from `docs/design/README`. No churn, but two places for designs.
- C: move only Communications and Text templating (the ones with code or started work) now, the rest later.

**Default:** A once you confirm; nothing moves until then.

**Your answer:**

### Q5 (2026-11-05, Communications) May `OoBDev.Communications` reference `OoBDev.System` for `AddValidatedOptions<T>()`?

The coding standard says options validation uses `AddValidatedOptions<T>()`, which lives in `OoBDev.System`. `OoBDev.Communications` does not reference it, so `SendReceiveOptions` and the other options use data annotations with `ValidateOnStart` (same effect, different call). This is a layering question (rule `layering-and-migration-plans`).

**Options**
- A (recommended): keep the data-annotation form in Communications. No new dependency and the behaviour is the same; cost: one place that does not follow the standard call, noted in the readme.
- B: reference `OoBDev.System` and switch to `AddValidatedOptions<T>()`. One validation style everywhere; cost: Communications (and its adapters, through it) take on the whole `OoBDev.System` surface.
- C: move `AddValidatedOptions<T>()` into a small options-only Common project that both can reference. Cleanest layering; cost: a new project and a migration plan.

**Default:** A.

**Your answer:**

### Q6 (2026-11-05, Communications) How do email attachments travel in `IEmailMessage`?

The requirements put attachments in phase 2. Today `EmailMessageModel.Attachments` holds references (`AttachmentReferenceModel`: container and key) that nothing resolves, and `IEmailMessage` has no attachments. The send path needs bytes at the provider.

**Options**
- A (recommended): `IEmailAttachment` (name, content type, `Func<CancellationToken, Task<Stream>>` opener) on `IEmailMessage`; a resolver in the queue handler turns a stored reference into an opener once Epic 6 (Documents) has a store. Small messages work now and large files are not buffered; cost: the resolver waits for Epic 6.
- B: attachments as `byte[]` inline. Simplest, tested with MailKit now; cost: memory use and the 10 MB limit in the requirements apply to every send.
- C: references only, each provider fetches the document itself. No copy in the message; cost: every adapter needs store access (layering).

**Default:** A, built without the resolver; not started until you confirm.

**Your answer:**

### Q7 (2026-11-05, Communications) Which vendor gets the first provider adapter and webhook processor?

Only MailKit (SMTP and IMAP) exists. The design names SendGrid and Twilio; Azure Communication Services and AWS SES/SNS also fit the repo's cloud adapters. An adapter and its webhook processor can only be Integration-tested with an emulator or a real account (Moto covers AWS).

**Options**
- A (recommended): AWS SES (send) plus SNS delivery notifications as the webhook processor: Moto is already in the test stack, so Integration tests run locally without an account.
- B: SendGrid (email) then Twilio (SMS): the designs' examples; cost: only live tests (`LiveIntegration`, manual) because there is no emulator.
- C: Azure Communication Services: matches the Azure adapters; cost: no emulator either.

**Default:** A once you confirm.

**Your answer:**
