// UserPromptSubmit hook: a sign-off ("working out", "heading out", ...) starts autonomous mode (.claude/rules/autonomous-mode.md)
// by creating .claude/autonomous.on; "I'm back" ends it.
import { readFileSync, writeFileSync, rmSync, mkdirSync } from 'node:fs';
import { join } from 'node:path';

let prompt = '';
try {
  prompt = JSON.parse(readFileSync(0, 'utf8')).prompt ?? '';
} catch {
  process.exit(0);
}

const dir = join(process.env.CLAUDE_PROJECT_DIR ?? process.cwd(), '.claude');
const flag = join(dir, 'autonomous.on');
// "I'm back" counts only as a plain statement, not when quoted or described ("when I want you to stop I will say \"I'm back\"").
const back = /(?<![“"'‘]|\b(say|says|saying|said|type|types) )\bi.?m back\b(?![”"'’])|(?<![“"'‘]|\b(say|says|saying|said|type|types) )\bi am back\b(?![”"'’])/i;
const signOff = /work(ing)? out|going to (the )?gym|working without|(don'?t|do not|never|stop) stop(ping)?|stop stopping|keep (going|working|at it)|keep go\b|until (the )?(back ?(lock|log)|backlog|todo|list|queue) is empty|until (everything|it) is blocked|non-?stop|work autonomously|without (any )?questions|i'?m (away|leaving|heading out|going out|going to (bed|lunch|sleep|dinner))|going to (bed|lunch|sleep|dinner)|heading out|back later|\bbe back\b|\bafk\b/i;

let context = '';
if (back.test(prompt)) {
  rmSync(flag, { force: true });
  context = 'OWNER IS BACK (autonomous mode ended): stop the backlog loop, give one short summary (done, parked, assumptions), then run the owner-interview skill on the open questions.';
} else if (signOff.test(prompt)) {
  mkdirSync(dir, { recursive: true });
  writeFileSync(flag, new Date().toISOString());
  context = 'AUTONOMOUS MODE (owner sign-off detected, .claude/rules/autonomous-mode.md): start it now without asking. Work TODO.md and BACKLOG.md in the work-backlog order, one commit and push of the working branch per closed item with tests, docs and tracking. No progress reports or updates of any kind, no check-in questions, no stopping to summarize. A blocked item is parked and you move to another. Park owner decisions in OPEN-QUESTIONS.md with a recommendation and carry on. Never merge to main or touch upstream. The mode ends only when the owner says I am back, or when every remaining item is parked in OPEN-QUESTIONS.md because it needs an owner answer. Hardware, a Linux container, a Docker stack, a long job, size or effort are NOT blocks: do them. The Stop hook keeps the turn open while .claude/autonomous.on exists.';
}

if (context)
  console.log(JSON.stringify({ hookSpecificOutput: { hookEventName: 'UserPromptSubmit', additionalContext: context } }));
