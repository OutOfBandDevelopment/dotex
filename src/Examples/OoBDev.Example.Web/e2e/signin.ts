import type { Page } from '@playwright/test';

export const testUser = { name: process.env.E2E_USER ?? 'ada', password: process.env.E2E_PASSWORD ?? 'ada' };

/** Completes the Keycloak login form when the stack redirected there. */
export async function signInIfAsked(page: Page): Promise<void> {
  if (!/\/realms\//.test(page.url())) {
    try { await page.waitForURL(/\/realms\//, { timeout: 1500 }); } catch { return; }
  }
  await page.locator('#username').fill(testUser.name);
  await page.locator('#password').fill(testUser.password);
  await page.locator('#kc-login').click();
  await page.waitForURL(url => !/\/realms\//.test(url.href));
}
