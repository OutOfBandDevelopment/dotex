# Feature request: culture negotiation middleware

- **Status:** done (built 2026-11-17, see [change](../../changes/2026-11-17.md#culture-negotiation-options-and-translate-helper))
- **Requested by:** owner (from `docs/Framework/CultureInfoMiddleware.md`, removed 2026-11-10)
- **Date:** 2026-11-10
- **Area:** ASP.NET Core middleware, text templating

## Need

An older design described middleware that exchanges language and culture between client and server. `ICultureInfoAccessor` is not in `src` now, so the feature is gone or was never carried over.

## Proposed behavior

The client sends `Accept-Language` on every request; the server resolves a culture, exposes it through `ICultureInfoAccessor`, and answers with `Content-Language`. Services take the accessor by constructor injection. Templates and a server-side resource translation (key and culture to value) use the same culture.

## Acceptance criteria

- Given `Accept-Language: fr-CA`, when a request is handled, then `ICultureInfoAccessor` returns `fr-CA` and the response carries `Content-Language: fr-CA`.
- Given an unsupported culture, then the configured default is used and returned.
- Given a Handlebars template using a translation helper, then the value comes from the request culture.

## Existing code checked

Searched `src` for `ICultureInfoAccessor`: not found. ASP.NET Core ships `RequestLocalizationMiddleware`, which may cover negotiation; the open part is the accessor and template integration.

## Security and operations

None; no data stored.

## Priority

- **Impact:** localised output; none needed today.
- **Needed by:** no date
- **Workaround today:** `RequestLocalizationMiddleware`

## Open questions

Use the platform middleware only, or keep the accessor abstraction?

## Triage notes

References: MDN pages for `Accept-Language` and `Content-Language`.

2026-11-11: triaged. `ICultureInfoAccessor` is not in `src`. Plan: use ASP.NET Core `RequestLocalizationMiddleware` for negotiation and add only the accessor and template integration if a consumer needs it. Backlog: BACKLOG.md, Framework wish list.

## Outcome

Decision: keep the accessor (`IAccessor<CultureInfo>`, already registered) and the existing `CultureInfoMiddleware` instead of the platform `RequestLocalizationMiddleware`, so services and templates read one request-scoped culture. The middleware now negotiates `Accept-Language` by quality against `CultureInfoMiddlewareOptions` (`OoBDev:Culture`: `DefaultCulture`, `SupportedCultures`; registered with `TryAddCultureNegotiation(configuration)`), falls back to the default for an unsupported culture and returns the chosen one in `Content-Language`. `ITranslationProvider` (`OptionsTranslationProvider`, `TranslationOptions`, fr-CA then fr then neutral) and the Handlebars `translate` helper give template text in the request culture. Unit tests: 5 middleware, 5 translation, 1 helper.
