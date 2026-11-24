import { http } from './http';

/** The sign-in return page is not a screen the owner wants counted. */
const ignored = new Set(['/signin-callback']);

/** Reports one screen view to the aggregate counter; never blocks or fails navigation. */
export function trackScreen(path: string): void {
  if (ignored.has(path)) return;
  http.postJson('/api/v1/analytics/screen-views', { screen: path }).catch(() => { /* analytics must not affect the user */ });
}
