import { shallowRef } from 'vue';
import { UserManager, WebStorageStateStore, type User } from 'oidc-client-ts';
import type { AppConfig } from './config';

/** Authentication is optional: with no authority configured the API is called anonymously (IDENTITY_PROVIDER=None). */
export let authEnabled = false;

class InMemoryStore implements Storage {
  private data = new Map<string, string>();
  get length() { return this.data.size; }
  clear() { this.data.clear(); }
  getItem(key: string) { return this.data.get(key) ?? null; }
  key(index: number) { return [...this.data.keys()][index] ?? null; }
  removeItem(key: string) { this.data.delete(key); }
  setItem(key: string, value: string) { this.data.set(key, value); }
  [name: string]: unknown;
}

// Tokens stay in memory only.
let manager: UserManager | null = null;

/** Called once at start-up with the loaded configuration. */
export function configureAuth(settings: AppConfig): void {
  authEnabled = !!settings.oidcAuthority;
  manager = authEnabled
    ? new UserManager({
        authority: settings.oidcAuthority!,
        client_id: settings.oidcClientId ?? 'oobdev-example-web',
        redirect_uri: `${window.location.origin}/signin-callback`,
        post_logout_redirect_uri: window.location.origin,
        response_type: 'code',
        scope: 'openid profile email',
        userStore: new WebStorageStateStore({ store: new InMemoryStore() }),
        stateStore: new WebStorageStateStore({ store: window.sessionStorage }),
      })
    : null;
}

const current = shallowRef<User | null>(null);

export const auth = {
  get user() { return current.value; },
  get accessToken() { return current.value?.access_token ?? null; },
  async signIn(returnTo = window.location.pathname + window.location.search) { await manager?.signinRedirect({ state: returnTo }); },
  /** Completes the redirect and returns the path the visitor asked for before signing in. */
  async completeSignIn(): Promise<string> {
    current.value = (await manager?.signinRedirectCallback()) ?? null;
    return typeof current.value?.state === 'string' ? current.value.state : '/';
  },
  async signOut() { current.value = null; await manager?.signoutRedirect(); },
};
