# OoBDev.Example.Web

Vue 3 example front end with an Azure-portal-style left navigation. Each screen exercises one framework feature through `OoBDev.Example.WebApi`. Plan and checklist: [future-example.md](../../../future-example.md); designs: [docs/design/example-ui](../../../docs/design/example-ui/README.md).

## Run

```bash
npm install
npm run dev        # http://localhost:5173, proxies /api to http://localhost:5000
npm test           # Vitest
npm run build      # type check and production build
```

Whole stack (SPA, API and backend services):

```bash
docker compose -f containers/example/docker-compose.example.yml up -d --build
```

The SPA is on `http://localhost:8088` (`EXAMPLE_PORT_WEB`).

## Conventions

- Fluent UI web components for controls; the shell (`src/App.vue`, `src/layout`) is custom.
- Each screen has a view-model (`*ViewModel.ts`) and real command classes derived from `src/core/command.ts`; templates bind to them.
- Sign-in uses `oidc-client-ts` (authorization code with PKCE) when `VITE_OIDC_AUTHORITY` is set; otherwise calls are anonymous. Tokens are kept in memory only.
- Errors from the API are RFC 9457 problem details (`src/core/http.ts`).
- Add a screen: view-model, page, route in `src/router`, entry in `src/layout/nav.ts`.
