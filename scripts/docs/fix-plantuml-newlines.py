#!/usr/bin/env python3
"""Repair PlantUML blocks whose quoted labels/messages contain literal line breaks
(a common authoring slip) by converting them to \n escapes. Usage: fix-plantuml-newlines.py <files...>"""
import sys
for f in sys.argv[1:]:
    lines = open(f, encoding='utf-8', newline='').read().split('\n')
    out, inblk, buf, changed = [], False, None, False
    for l in lines:
        if not inblk:
            out.append(l); inblk = l.startswith('```plantuml'); continue
        if l.startswith('```'):
            if buf is not None: out.append(buf); buf = None
            out.append(l); inblk = False; continue
        cur = l if buf is None else buf + '\n' + l
        if buf is not None: changed = True
        # message continuation: line starting with '(' or lowercase after a message label
        if buf is None and out and inblk and l.startswith('(') and ' : ' in out[-1]:
            out[-1] += '\n' + l; changed = True; continue
        if cur.count('"') % 2: buf = cur
        else: out.append(cur); buf = None
    if changed: open(f, 'w', encoding='utf-8', newline='').write('\n'.join(out)); print('fixed', f)
