import { describe, expect, it } from 'vitest';
import { Command } from './command';

class Fails extends Command {
  protected async run(): Promise<void> {
    throw new Error('boom');
  }
}

class Counts extends Command<number> {
  total = 0;
  protected async run(n: number): Promise<void> {
    this.total += n;
  }
}

describe('Command', () => {
  it('execute_Failure_CapturesErrorAndStopsRunning', async () => {
    const c = new Fails();
    await c.execute();
    expect(c.error.value).toBe('boom');
    expect(c.running.value).toBe(false);
  });

  it('execute_Success_RunsWithArgument', async () => {
    const c = new Counts();
    await c.execute(3);
    expect(c.total).toBe(3);
    expect(c.error.value).toBeNull();
  });
});
