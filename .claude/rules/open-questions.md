# Open questions (always apply)

- Questions for the owner are tracked in `/OPEN-QUESTIONS.md`. When a question comes up that you cannot settle from the request, code or sensible defaults, add it there (next `Q<n>`, date, area, question, the **Default** you will use meanwhile, an empty **Your answer:**), keep working on the default, and mention the new entry in your reply.
- Read the file at the start of a session and before starting work in an area it names. A non-empty **Your answer** is an answer: act on it, move the entry to **Answered** with the date and what you did, and update `TODO.md` if it changes the backlog.
- An answer given in chat counts the same as one written in the file. Copy it into the entry (quote it), move the entry to **Answered**, then act.
- A SessionStart hook (`.claude/hooks/open-questions.mjs`) reports the open and unprocessed counts; act on its report.
- Do not delete entries; answered ones stay under **Answered**. Do not reopen an answered question unless the owner asks.
- Do not ask the same question in chat again when it is already open in the file, unless the owner asks for a summary.
- The file lives at the repo root by owner request; it is the only root file besides `README.md`, `CLAUDE.md`, `TODO.md`, `TEST_VARIABLES.md` and `CONFIGURATION_SETTINGS.md`.
