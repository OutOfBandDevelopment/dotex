#!/usr/bin/env python3
"""Repair PlantUML blocks whose quoted labels/messages contain real line breaks
(a common authoring slip) by joining them with the PlantUML escape backslash-n.

Usage: python scripts/docs/fix-plantuml-newlines.py <files...>
Deterministic; safe to re-run (idempotent).
"""
import sys

NL = chr(10)
ESC = chr(92) + 'n'  # backslash + n as PlantUML expects


def fix(text):
    lines = text.split(NL)
    out, inblk, buf, changed = [], False, None, False
    for l in lines:
        if not inblk:
            out.append(l)
            inblk = l.startswith('```plantuml')
            continue
        if l.startswith('```'):
            if buf is not None:
                out.append(buf)
                buf = None
            out.append(l)
            inblk = False
            continue
        # a message label continued on the next line: "A -> B : text" + "(more)"
        if buf is None and l.startswith('(') and out and ' : ' in out[-1]:
            out[-1] += ESC + l
            changed = True
            continue
        cur = l if buf is None else buf + ESC + l
        if buf is not None:
            changed = True
        if cur.count('"') % 2:
            buf = cur
        else:
            out.append(cur)
            buf = None
    return NL.join(out), changed


if __name__ == '__main__':
    for f in sys.argv[1:]:
        text = open(f, encoding='utf-8', newline='').read()
        new, changed = fix(text)
        if changed:
            open(f, 'w', encoding='utf-8', newline='').write(new)
            print('fixed', f)
