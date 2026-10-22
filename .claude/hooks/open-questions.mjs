// SessionStart hook: tell Claude how many owner questions in OPEN-QUESTIONS.md are still unanswered
// and which have an answer waiting to be processed.
import { readFileSync } from 'node:fs';
import { join } from 'node:path';

const file = join(process.env.CLAUDE_PROJECT_DIR ?? process.cwd(), 'OPEN-QUESTIONS.md');
let text;
try {
  text = readFileSync(file, 'utf8');
} catch {
  process.exit(0);
}

const openStart = text.indexOf('\n## Open');
const answeredStart = text.indexOf('\n## Answered');
if (openStart < 0) process.exit(0);
const open = text.slice(openStart, answeredStart < 0 ? undefined : answeredStart);

const entries = open.split(/\n### /).slice(1);
const waiting = [];
const pending = [];
for (const entry of entries) {
  const id = entry.split('\n')[0].split(' ')[0];
  const answer = /\*\*Your answer:\*\*[ \t]*(.*)/.exec(entry)?.[1]?.trim() ?? '';
  (answer ? waiting : pending).push(id);
}

if (entries.length === 0) process.exit(0);
const lines = [`OPEN-QUESTIONS.md: ${pending.length} open without an answer (${pending.join(', ') || 'none'}).`];
if (waiting.length > 0)
  lines.push(`Answered by the owner but not processed yet: ${waiting.join(', ')}. Act on them, then move them to Answered (rule open-questions).`);

console.log(JSON.stringify({
  hookSpecificOutput: { hookEventName: 'SessionStart', additionalContext: lines.join(' ') },
}));
