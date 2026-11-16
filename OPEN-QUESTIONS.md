# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q-OFFICE-MARKDOWN — 2026-11-16 — DocumentToMarkdown (layering)

Office files (docx, odt, rtf, epub, doc) to Markdown need a reader behind `IFileMarkdownReader` (built 2026-11-16, HTML first). The only office converter in the repo is Apache Tika (a server, async HTTP, output HTML), and HTML to Markdown is `OoBDev.MysticMind`. Adapters reference Abstractions only, so neither can call the other.

**Options**
- A (recommended): a small Extensions or Framework composition project `OoBDev.Documents.Markdown.Office` that implements `IFileMarkdownReader` by running the registered `IDocumentConversionHandler` pair (Tika to HTML, then MysticMind HTML to Markdown) through the existing conversion pipeline. No new third-party package, Tika stays optional, needs a Tika container (already in the stack). Reader runs async work synchronously (the interface is sync).
- B: a new `IFileMarkdownReader` in the Tika adapter that asks Tika for plain text only. Simplest, loses headings, lists and tables.
- C: add a direct OpenXml/NPOI package reader (no server). Best fidelity offline, new dependency to approve (rule `coding-standards`), more code per format.

**Default:** A once confirmed; nothing built meanwhile (the item is parked).
**Your answer:**
