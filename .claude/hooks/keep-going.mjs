// Stop hook: while autonomous mode is on (.claude/autonomous.on), do not let the turn end (.claude/rules/autonomous-mode.md).
// Claude deletes the flag itself only after the close-out audit finds nothing new and every remaining item is parked in OPEN-QUESTIONS.md.
import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';

const root = process.env.CLAUDE_PROJECT_DIR ?? process.cwd();
if (!existsSync(join(root, '.claude', 'autonomous.on'))) process.exit(0);

try {
  readFileSync(0, 'utf8');
} catch {
  // stdin is optional
}

console.log(JSON.stringify({
  decision: 'block',
  reason: 'Autonomous mode is on until the owner says I am back (.claude/rules/autonomous-mode.md). Do not stop and do not write a status message. Re-read OPEN-QUESTIONS.md for new owner requests under ## Requests and answers. Take the next unblocked item from TODO.md, then BACKLOG.md, and work it with tests, docs and tracking. If an item needs an answer, add it to OPEN-QUESTIONS.md and take another. Only when the close-out audit finds nothing new and every remaining item is parked, delete .claude/autonomous.on and end the turn.',
}));
