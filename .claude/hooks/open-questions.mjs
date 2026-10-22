// SessionStart hook: tell Claude how many owner questions in OPEN-QUESTIONS.md are still unanswered
// and which have an answer waiting to be processed, and how many owner requests under "## Requests" are waiting to be merged into the docs and backlog.
import { readFileSync } from 'node:fs';
import { join } from 'node:path';

const file = join(process.env.CLAUDE_PROJECT_DIR ?? process.cwd(), 'OPEN-QUESTIONS.md');
let text;
try {
  text = readFileSync(file, 'utf8');
} catch {
  process.exit(0);
}

const reqStart = text.indexOf('\n## Requests');
const reqEnd = text.indexOf('\n## ', reqStart + 1);
const requests = reqStart < 0 ? [] : text.slice(reqStart, reqEnd < 0 ? undefined : reqEnd).split('\n').filter(l => /^\s*[-*]\s+\S/.test(l));

const openStart = text.indexOf('\n## Open');
const answeredStart = text.indexOf('\n## Answered');
if (openStart < 0 && requests.length === 0) process.exit(0);
const open = openStart < 0 ? '' : text.slice(openStart, answeredStart < 0 ? undefined : answeredStart);

const entries = open.split(/\n### /).slice(1);
const waiting = [];
const pending = [];
for (const entry of entries) {
  const id = entry.split('\n')[0].split(' ')[0];
  const answer = /\*\*Your answer:\*\*[ \t]*(.*)/.exec(entry)?.[1]?.trim() ?? '';
  (answer ? waiting : pending).push(id);
}

if (entries.length === 0 && requests.length === 0) process.exit(0);
const lines = [`OPEN-QUESTIONS.md: ${pending.length} open without an answer (${pending.join(', ') || 'none'}).`];
if (requests.length > 0)
  lines.push(`${requests.length} owner request(s) under "## Requests" are not processed yet: treat each as a feature request, merge it into the right docs and BACKLOG.md, then remove it from the list (rule open-questions).`);
if (waiting.length > 0)
  lines.push(`Answered by the owner but not processed yet: ${waiting.join(', ')}. Act on them, then move them to removed after you record them (rule open-questions).`);

console.log(JSON.stringify({
  hookSpecificOutput: { hookEventName: 'SessionStart', additionalContext: lines.join(' ') },
}));
