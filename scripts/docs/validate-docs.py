#!/usr/bin/env python3
"""Validate markdown docs: PlantUML renders, relative links resolve, captions are sequential.

Usage: python scripts/docs/validate-docs.py [paths...]   (default: docs/)
Renders diagrams with the plantuml/plantuml-server docker image (started automatically,
left running for reuse; stop with: docker rm -f dotex-plantuml). Exit code 1 on any error.
Options: --no-render (skip PlantUML rendering), --port N (default 18080), --svg-dir DIR (save SVGs).
"""
import argparse, os, re, subprocess, sys, time, urllib.request, urllib.error

NAME = 'dotex-plantuml'
FENCE = re.compile(r'^(`{3,})(\w*)\s*$')

def md_files(paths):
    for p in paths:
        if os.path.isfile(p): yield p
        else:
            for r, _, fs in os.walk(p):
                for f in fs:
                    if f.lower().endswith('.md'): yield os.path.join(r, f)

def parse(path):
    """Yield (kind, start_line, text) for plantuml blocks; and non-fenced lines list."""
    blocks, prose, cur, fence = [], [], None, None
    for i, l in enumerate(open(path, encoding='utf-8').read().split('\n'), 1):
        m = FENCE.match(l)
        if fence is None and m:
            fence = m.group(1); cur = [m.group(2), i, []]; continue
        if fence is not None:
            if l.strip() == fence:
                if cur[0] == 'plantuml': blocks.append((cur[1], '\n'.join(cur[2])))
                fence = None; continue
            cur[2].append(l); continue
        prose.append((i, l))
    return blocks, prose

def ensure_server(port):
    def up():
        try: urllib.request.urlopen(f'http://localhost:{port}/txt/~h407374617274756d6c0a4020656e64756d6c', timeout=3); return True
        except urllib.error.HTTPError: return True
        except Exception: return False
    if up(): return
    subprocess.run(['docker', 'rm', '-f', NAME], capture_output=True)
    r = subprocess.run(['docker', 'run', '-d', '--name', NAME, '-p', f'{port}:8080', 'plantuml/plantuml-server:jetty'], capture_output=True, text=True)
    if r.returncode: sys.exit('docker failed: ' + r.stderr)
    for _ in range(60):
        if up(): return
        time.sleep(1)
    sys.exit('plantuml server did not start')

def render(port, text):
    """Return (ok, message, svg)."""
    enc = '~h' + text.encode('utf-8').hex()
    try:
        svg = urllib.request.urlopen(f'http://localhost:{port}/svg/{enc}', timeout=60).read().decode('utf-8')
        err = 'Syntax Error' in svg
        return (not err, 'Syntax Error in rendered output' if err else '', svg)
    except urllib.error.HTTPError as e:
        body = e.read().decode('utf-8', 'ignore')
        try:  # ask for the text form to get the error line
            urllib.request.urlopen(f'http://localhost:{port}/txt/{enc}', timeout=60).read()
        except Exception: pass
        m = re.search(r'Syntax Error\??[^<]*|<text[^>]*>([^<]*line[^<]*)</text>', body)
        lines = re.findall(r'>([^<>]*(?:line|Error|error|Cannot|Assumed)[^<>]*)<', body)
        return False, f'HTTP {e.code} ' + ' | '.join(lines[:3]), body

def check(files, render_on, port, svg_dir):
    errors = 0
    if render_on: ensure_server(port)
    for f in files:
        blocks, prose = parse(f)
        rel = os.path.relpath(f)
        for start, text in blocks:
            t = text.strip()
            if not re.match(r'@start(uml|salt)\b', t) or not re.search(r'@end(uml|salt)\s*$', t):
                print(f'{rel}:{start}: plantuml block must start with @startuml/@startsalt and end with matching @end'); errors += 1; continue
            if re.search(r'^\s*!include(url|sub)?\s+.*(C4|https?://)', text, re.M):
                print(f'{rel}:{start}: remote/C4 !include is not allowed (use plain PlantUML)'); errors += 1
            if '\n' not in text and re.search(r'\[[^\]\n]*$', text, re.M):
                print(f'{rel}:{start}: unbalanced [ on a line (literal newline inside brackets?)'); errors += 1
            if render_on:
                ok, msg, svg = render(port, text)
                if not ok: print(f'{rel}:{start}: render failed: {msg}'); errors += 1
                elif svg_dir:
                    os.makedirs(svg_dir, exist_ok=True)
                    open(os.path.join(svg_dir, f'{os.path.basename(f)}-{start}.svg'), 'w', encoding='utf-8').write(svg)
        # links
        for i, l in prose:
            for m in re.finditer(r'\]\((?!https?:|mailto:|#)([^)#\s]+)(#[^)\s]*)?\)', l):
                t = os.path.normpath(os.path.join(os.path.dirname(f), m.group(1)))
                if not os.path.exists(t): print(f'{rel}:{i}: broken link {m.group(1)}'); errors += 1
        # captions sequential
        for kind, pat in (('Figure', r'^\*Figure (\d+) — '), ('Table', r'^\*\*Table (\d+) — ')):
            nums = [int(m.group(1)) for _, l in prose for m in [re.match(pat, l)] if m]
            if nums and nums != sorted(nums): print(f'{rel}: {kind} captions out of order {nums}'); errors += 1
    return errors

if __name__ == '__main__':
    ap = argparse.ArgumentParser(); ap.add_argument('paths', nargs='*', default=['docs'])
    ap.add_argument('--no-render', action='store_true'); ap.add_argument('--port', type=int, default=18080)
    ap.add_argument('--svg-dir'); a = ap.parse_args()
    fs = sorted(md_files(a.paths)); n = check(fs, not a.no_render, a.port, a.svg_dir)
    print(f'{len(fs)} files checked, {n} problem(s)'); sys.exit(1 if n else 0)
