// PreToolUse hook for Bash: enforces the git-workflow rule.
// Blocks AI attribution in commit messages, force pushes, gh usage and pushes to upstream (GitHub).
// Exit code 2 blocks the call and returns stderr to the model.
import { readFileSync } from 'node:fs';

let input;
try {
  input = JSON.parse(readFileSync(0, 'utf8'));
} catch {
  process.exit(0);
}

const command = input?.tool_input?.command ?? '';

// A command word must start a command (line start or after ; & | or an open parenthesis),
// so text such as "dotex.git" inside a heredoc or an argument is not mistaken for a command.
const START = '(?:^|[\\n;&|(])\\s*';
const gitCommand = (subcommand) => new RegExp(`${START}git\\b[^\\n|;&]*\\b${subcommand}\\b`).test(command);

const block = (message) => {
  console.error(`guard-git: ${message}`);
  process.exit(2);
};

if (gitCommand('commit')) {
  const banned = [
    [/co-authored-by/i, 'Co-Authored-By trailer'],
    [/generated (with|by)/i, '"generated with/by" line'],
    [/noreply@anthropic\.com/i, 'Anthropic address'],
    [/\bAI[- ]generated\b/i, '"AI generated" text'],
    [/(?<![.\/\\\w-])Claude(?![\w.])/, 'mention of Claude'],
  ];
  for (const [pattern, label] of banned) {
    if (pattern.test(command)) {
      block(`${label} found in commit text. Commit messages must never reference Claude or AI (rule: .claude/rules/git-workflow.md). Remove it and retry.`);
    }
  }
}

if (gitCommand('push')) {
  if (/(\s--force(?!-)|--force-with-lease|\s-f(\s|$)|\s\+\S+)/.test(command)) {
    block('force push needs explicit owner approval in this turn. Ask first (rule: .claude/rules/git-workflow.md).');
  }
  if (/\bupstream\b/.test(command)) {
    block('do not push to upstream (GitHub); push to origin, the local bare repo (rule: .claude/rules/git-workflow.md).');
  }
}

if (new RegExp(`${START}gh\\s`).test(command)) {
  block('GitHub is no longer the primary repo; do not use gh. The primary repo is the local bare repo `origin` (rule: .claude/rules/git-workflow.md).');
}

process.exit(0);
