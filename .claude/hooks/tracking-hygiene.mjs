// SessionStart hook: reports when the tracking files drift from the work-tracking rule
// (TODO.md too long or with run-on items, stale "Last Updated" dates, a stale priorities list in CLAUDE.md).
// It only adds context; it never blocks.
import { readFileSync } from 'node:fs';
import { join } from 'node:path';

const root = process.env.CLAUDE_PROJECT_DIR ?? process.cwd();
const read = (name) => {
  try {
    return readFileSync(join(root, name), 'utf8').replace(/\r\n/g, '\n');
  } catch {
    return null;
  }
};

const MAX_TODO_ITEMS = 15;
const MAX_TODO_ITEM_CHARS = 400;
const MAX_BACKLOG_ITEM_CHARS = 700;
const MAX_AGE_DAYS = 3;
const today = new Date();
const problems = [];

const items = (text) => text.split('\n').filter((l) => /^\s*- \[[ x]\]/.test(l));
const ageDays = (text) => {
  const m = /\*\*Last Updated:\*\*\s*(\d{4}-\d{2}-\d{2})/.exec(text);
  return m ? Math.floor((today - new Date(m[1])) / 86400000) : null;
};

const todo = read('TODO.md');
if (todo) {
  const list = items(todo);
  if (list.length > MAX_TODO_ITEMS) problems.push(`TODO.md has ${list.length} items (limit ${MAX_TODO_ITEMS}); move the rest to BACKLOG.md.`);
  const long = list.filter((l) => l.length > MAX_TODO_ITEM_CHARS).length;
  if (long > 0) problems.push(`TODO.md has ${long} item(s) over ${MAX_TODO_ITEM_CHARS} characters; keep one short line per item and put history in docs/changes/ and the feature doc.`);
  const age = ageDays(todo);
  if (age !== null && age > MAX_AGE_DAYS) problems.push(`TODO.md "Last Updated" is ${age} days old; review it against the work done.`);
}

const backlog = read('BACKLOG.md');
if (backlog) {
  const long = items(backlog).filter((l) => l.length > MAX_BACKLOG_ITEM_CHARS).length;
  if (long > 0) problems.push(`BACKLOG.md has ${long} item(s) over ${MAX_BACKLOG_ITEM_CHARS} characters; split them into sub-items or move detail to the feature doc.`);
  const age = ageDays(backlog);
  if (age !== null && age > MAX_AGE_DAYS) problems.push(`BACKLOG.md "Last Updated" is ${age} days old; remove finished and duplicate items and refresh the date.`);
}

if (problems.length === 0) process.exit(0);
console.log(JSON.stringify({
  hookSpecificOutput: {
    hookEventName: 'SessionStart',
    additionalContext: `tracking-hygiene (rule work-tracking, "Keeping the files correct"): ${problems.join(' ')}`,
  },
}));
