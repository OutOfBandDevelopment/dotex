// PreToolUse hook for Write/Edit/MultiEdit: blocks ASCII-art diagrams in markdown (rule docs-and-diagrams,
// owner, 2026-11-05). Diagrams are PlantUML fenced blocks; mockups are PlantUML Salt. Directory trees are fine.
import { readFileSync } from 'node:fs';

let input;
try {
  input = JSON.parse(readFileSync(0, 'utf8'));
} catch {
  process.exit(0);
}

const ti = input?.tool_input ?? {};
const path = String(ti.file_path ?? '');
if (!/\.md$/i.test(path)) process.exit(0);

const texts = [];
if (typeof ti.content === 'string') texts.push(ti.content);
if (typeof ti.new_string === 'string') texts.push(ti.new_string);
for (const e of ti.edits ?? []) if (typeof e?.new_string === 'string') texts.push(e.new_string);

// Box corners and junctions, two or more vertical bars on one line, or +---+ borders: a drawn box, not a tree.
const boxLine = /[┌┐┘┬┴┼╔╗╚╝╠╣╦╩╬]|│[^│\n]*│|\+-{3,}\+/;
const offenders = [];
for (const text of texts) {
  let fence = null;
  for (const line of text.split('\n')) {
    const m = /^\s*(`{3,}|~{3,})\s*(\S*)/.exec(line);
    if (m) {
      fence = fence ? null : (m[2] || '').toLowerCase();
      continue;
    }
    if (fence !== null && !['plantuml', 'salt'].includes(fence) && boxLine.test(line)) offenders.push(line.trim());
    else if (fence === null && boxLine.test(line)) offenders.push(line.trim());
  }
}
if (offenders.length === 0) process.exit(0);

console.error(
  `no-ascii-diagrams: ${path} would get an ASCII-art diagram ("${offenders[0].slice(0, 60)}"). ` +
  'Draw it as an embedded PlantUML block (```plantuml ... @startuml; mockups use @startsalt) with a "*Figure N — title*" caption, ' +
  'plain PlantUML C4 style without !include (rule docs-and-diagrams). Directory trees may stay as code blocks.',
);
process.exit(2);
