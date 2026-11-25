/** Runtime settings served as /config.json (written by the container at start-up, empty in local development). */
export interface AppConfig {
  /** OIDC authority, for example http://localhost:8081/realms/oobdev-example. Empty means the API is called anonymously. */
  oidcAuthority?: string;
  oidcClientId?: string;
}

export let config: AppConfig = {};

export async function loadConfig(): Promise<AppConfig> {
  const fromEnv: AppConfig = {
    oidcAuthority: import.meta.env.VITE_OIDC_AUTHORITY as string | undefined,
    oidcClientId: import.meta.env.VITE_OIDC_CLIENT_ID as string | undefined,
  };
  try {
    const response = await fetch('/config.json', { headers: { Accept: 'application/json' } });
    const served = response.ok && (response.headers.get('Content-Type') ?? '').includes('json') ? await response.json() as AppConfig : {};
    config = { ...fromEnv, ...Object.fromEntries(Object.entries(served).filter(([, v]) => v)) };
  } catch {
    config = fromEnv;
  }
  return config;
}
