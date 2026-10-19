import { test } from '@playwright/test';
import { resolve } from 'node:path';
import { imageDir, resetImages, stepImageName, writeGuide, writeIndexes, writeSpec } from './guide';
import { journeys } from './journeys';
import { mockApi } from './mocks';

// Each journey is one test: it walks the steps, takes a screenshot after every step for the user guide,
// and rewrites the guide and the spec from the same definition.
for (const journey of journeys) {
  test(`journey: ${journey.title}`, async ({ page }) => {
    if (!process.env.E2E_BASE_URL) await mockApi(page);
    resetImages(journey);
    const dir = imageDir(journey);

    await page.goto(journey.start);
    for (const [i, step] of journey.steps.entries()) {
      await test.step(step.title, async () => {
        await step.run(page);
        await page.waitForLoadState('networkidle');
        // Park the pointer and drop focus so hover and focus styling cannot make screenshots differ between runs.
        await page.mouse.move(0, 0);
        await page.evaluate(() => (document.activeElement as HTMLElement | null)?.blur());
        await page.waitForTimeout(300);
        await page.screenshot({ path: resolve(dir, stepImageName(i, step.title)), caret: 'hide', animations: 'disabled' });
      });
    }

    writeGuide(journey);
    writeSpec(journey);
  });
}

test.afterAll(() => writeIndexes(journeys));
