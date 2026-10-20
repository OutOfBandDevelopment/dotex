import type { Page } from '@playwright/test';

/** One step of a user journey: what the user does, what they should see, and the check that proves it. */
export interface Step {
  title: string;
  /** What the user does and why, written for the user guide. */
  narrative: string;
  /** Executable acceptance criteria shown in the spec; each is also asserted by `run`. */
  expect: string[];
  run: (page: Page) => Promise<void>;
}

/** One troubleshooting entry of a run book. */
export interface Problem {
  symptom: string;
  cause: string;
  fix: string;
}

/** A goal-oriented path through the application. Drives the test, the screenshots, the guide and the spec. */
export interface Journey {
  id: string;
  title: string;
  persona: string;
  goal: string;
  /** Route the journey starts on. */
  start: string;
  prerequisites: string[];
  /** Related framework features (links the guide back to the framework docs). */
  features: string[];
  /** Why a developer would use the feature the journey shows (shown in the guide and the features page). */
  why: string;
  /** Known problems for the run book: how they look, why they happen, how to fix them. */
  problems: Problem[];
  /** Needs the real stack (identity provider); skipped when the API is mocked. */
  liveOnly?: boolean;
  steps: Step[];
}
