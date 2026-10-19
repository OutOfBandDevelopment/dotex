import { UserManager, WebStorageStateStore, type User } from 'oidc-client-ts';

const authority = import.meta.env.VITE_OIDC_AUTHORITY as string | undefined;
const clientId = (import.meta.env.VITE_OIDC_CLIENT_ID as string | undefined) ?? 'oobdev-example-web';

/** Authentication is optional: with no authority configured the API is called anonymously (IDENTITY_PROVIDER=None). */
export const authEnabled = !!authority;

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
const manager = authEnabled
  ? new UserManager({
      authority: authority!,
      client_id: clientId,
      redirect_uri: `${window.location.origin}/signin-callback`,
      post_logout_redirect_uri: window.location.origin,
      response_type: 'code',
      scope: 'openid profile email',
      userStore: new WebStorageStateStore({ store: new InMemoryStore() }),
    })
  : null;

let current: User | null = null;

export const auth = {
  get user() { return current; },
  get accessToken() { return current?.access_token ?? null; },
  async signIn() { await manager?.signinRedirect(); },
  async completeSignIn() { current = (await manager?.signinRedirectCallback()) ?? null; },
  async signOut() { await manager?.signoutRedirect(); },
};
