// PreToolUse hook for Bash: enforces the work-tracking rule at commit time.
// When a commit stages code under src/ but no tracking file (TODO.md, BACKLOG.md, docs/changes, docs/design,
// a README.*.md), it adds a reminder to the model's context. It never blocks the commit.
import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';

let input;
try {
  input = JSON.parse(readFileSync(0, 'utf8'));
} catch {
  process.exit(0);
}

const command = input?.tool_input?.command ?? '';
const START = '(?:^|[\\n;&|(])\\s*';
if (!new RegExp(`${START}git\\b[^\\n|;&]*\\bcommit\\b`).test(command)) process.exit(0);

let staged;
try {
  staged = execFileSync('git', ['diff', '--cached', '--name-only'], {
    cwd: process.env.CLAUDE_PROJECT_DIR ?? process.cwd(),
    encoding: 'utf8',
  }).split('\n').filter(Boolean);
} catch {
  process.exit(0);
}

// One change file per day (owner, 2026-11-04): a new or edited docs/changes file must be README.md or {YYYY-MM-DD}.md.
let added = [];
try {
  added = execFileSync('git', ['diff', '--cached', '--name-only', '--diff-filter=AMR'], {
    cwd: process.env.CLAUDE_PROJECT_DIR ?? process.cwd(),
    encoding: 'utf8',
  }).split('\n').filter(Boolean);
} catch {
  added = [];
}
const badChange = added.filter((f) => /^docs\/changes\/[^/]+\.md$/.test(f) && !/^docs\/changes\/(README|\d{4}-\d{2}-\d{2})\.md$/.test(f));
if (badChange.length > 0) {
  console.log(JSON.stringify({
    hookSpecificOutput: {
      hookEventName: 'PreToolUse',
      additionalContext:
        `work-tracking: ${badChange.join(', ')} is not a daily change file. Change history is one file per day, docs/changes/{YYYY-MM-DD}.md, with a ## section per task (rule work-tracking, skill complete-work); move the section into that day's file.`,
    },
  }));
  process.exit(0);
}

// A new task section belongs in today's file (owner, 2026-11-05): the local date decides, not the newest file.
const pad = (n) => String(n).padStart(2, '0');
const now = new Date();
const today = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;
const wrongDay = [];
for (const f of added) {
  const m = /^docs\/changes\/(\d{4}-\d{2}-\d{2})\.md$/.exec(f);
  if (!m) continue;
  let diff = '';
  try {
    diff = execFileSync('git', ['diff', '--cached', '-U0', '--', f], {
      cwd: process.env.CLAUDE_PROJECT_DIR ?? process.cwd(),
      encoding: 'utf8',
    });
  } catch {
    continue;
  }
  const headings = diff.split('\n').filter((l) => /^\+## /.test(l) && !/^\+## (Contents|By day)\b/.test(l));
  const datedOther = headings.filter((l) => /\(\d{4}-\d{2}-\d{2}\)\s*$/.test(l) && !l.includes(`(${m[1]})`));
  if ((headings.length > 0 && m[1] !== today) || datedOther.length > 0) wrongDay.push(`${f} (today is ${today})`);
}
if (wrongDay.length > 0) {
  console.log(JSON.stringify({
    hookSpecificOutput: {
      hookEventName: 'PreToolUse',
      additionalContext:
        `work-tracking: ${wrongDay.join(', ')} gets a new task section whose date is not that file's day. A finished task goes in docs/changes/${today}.md (the system date, not the newest existing file); create it with the title, contents list and a By day line in README.md when the date has rolled over.`,
    },
  }));
  process.exit(0);
}

const code = staged.filter((f) => /^src\/.*\.(cs|csproj|props)$/.test(f));
const tracking = staged.filter((f) => /^(TODO\.md|BACKLOG\.md|OPEN-QUESTIONS\.md)$|^docs\/(changes|design)\/|(^|\/)README\.[^/]+\.md$/.test(f));
if (code.length === 0 || tracking.length > 0) process.exit(0);

console.log(JSON.stringify({
  hookSpecificOutput: {
    hookEventName: 'PreToolUse',
    additionalContext:
      `work-tracking: this commit stages ${code.length} code file(s) under src/ and no TODO.md, BACKLOG.md, docs/changes, docs/design or README file. ` +
      'If the commit finishes or moves an item, follow the work-tracking rule and the complete-work skill in the same commit series; otherwise carry on.',
  },
}));
