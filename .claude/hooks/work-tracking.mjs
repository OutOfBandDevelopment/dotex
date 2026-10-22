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
