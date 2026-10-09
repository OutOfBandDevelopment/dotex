#!/usr/bin/env python3
"""Generate navigation for a split document folder (see docs/patterns-discovery/06-design-document-standard.md).

For each folder given, files named NN-*.md are the sections and README.md is the index.
  * every section gets a nav line (up / previous / next) directly under its H1, and at the end
  * figure/table captions are renumbered continuously across the folder
  * README.md gets Contents + List of Figures + List of Tables between the markers
        <!-- toc:start -->  ...  <!-- toc:end -->
    (add the markers once, below the intro text; the script fills them)

Usage: python scripts/docs/build-index.py docs/some-topic [more folders...]
Idempotent and deterministic.
"""
import os
import re
import sys

NL = chr(10)
FIG = re.compile(r'^\*Figure \d+ — (.*)\*$')
TAB = re.compile(r'^\*\*Table \d+ — (.*)\*\*$')
NAV_MARK = '<!-- nav -->'


def sections(folder):
    return sorted(f for f in os.listdir(folder) if re.match(r'\d\d-.*\.md$', f))


def title_of(text):
    return text.split(NL, 1)[0].lstrip('# ').strip()


def strip_nav(lines):
    out, skip = [], False
    for l in lines:
        if l == NAV_MARK:
            skip = not skip
            continue
        if not skip:
            out.append(l)
    # remove the trailing rule left by the previous run
    while out and out[-1] == '':
        out.pop()
    if out and out[-1] == '---':
        out.pop()
    while out and out[-1] == '':
        out.pop()
    # collapse the blank lines left after the H1
    while len(out) > 1 and out[1] == '':
        out.pop(1)
    return out


def process(folder):
    files = sections(folder)
    readme_path = os.path.join(folder, 'README.md')
    readme = open(readme_path, encoding='utf-8').read()
    readme_title = title_of(readme)
    figs, tabs, toc = [], [], []
    fig_n = tab_n = 0
    contents = {}
    for f in files:
        lines = open(os.path.join(folder, f), encoding='utf-8').read().split(NL)
        lines = strip_nav(lines)
        in_fence = False
        for i, l in enumerate(lines):
            if l.startswith('```'):
                in_fence = not in_fence
            if in_fence:
                continue
            m = FIG.match(l)
            if m:
                fig_n += 1
                lines[i] = f'*Figure {fig_n} — {m.group(1)}*'
                figs.append((fig_n, m.group(1), f))
            m = TAB.match(l)
            if m:
                tab_n += 1
                lines[i] = f'**Table {tab_n} — {m.group(1)}**'
                tabs.append((tab_n, m.group(1), f))
        contents[f] = lines
        toc.append((f, title_of(NL.join(lines))))
    for idx, f in enumerate(files):
        prev = f'[← {toc[idx-1][1]}](./{toc[idx-1][0]})' if idx else '[← Index](./README.md)'
        nxt = f'[{toc[idx+1][1]} →](./{toc[idx+1][0]})' if idx + 1 < len(files) else '[Index →](./README.md)'
        nav = f'[↑ {readme_title}](./README.md) · {prev} · {nxt}'
        lines = contents[f]
        out = [lines[0], '', NAV_MARK, nav, NAV_MARK, ''] + lines[1:] + ['', '---', '', NAV_MARK, nav, NAV_MARK, '']
        open(os.path.join(folder, f), 'w', encoding='utf-8', newline='').write(NL.join(out))
    block = ['<!-- toc:start -->', '## Contents', '']
    block += [f'{i}. [{t}](./{f})' for i, (f, t) in enumerate(toc, 1)]
    if figs:
        block += ['', '### List of Figures', '']
        block += [f'{n}. [Figure {n} — {t}](./{f})' for n, t, f in figs]
    if tabs:
        block += ['', '### List of Tables', '']
        block += [f'{n}. [Table {n} — {t}](./{f})' for n, t, f in tabs]
    block += ['<!-- toc:end -->']
    if '<!-- toc:start -->' not in readme:
        sys.exit(f'{readme_path}: add <!-- toc:start --> and <!-- toc:end --> markers')
    new = re.sub(r'<!-- toc:start -->.*?<!-- toc:end -->', lambda m: NL.join(block), readme, flags=re.S)
    open(readme_path, 'w', encoding='utf-8', newline='').write(new)
    print(f'{folder}: {len(files)} sections, {len(figs)} figures, {len(tabs)} tables')


if __name__ == '__main__':
    for d in sys.argv[1:]:
        process(d)
