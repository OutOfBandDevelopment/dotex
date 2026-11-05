#!/bin/sh
# PreToolUse hook for Bash: on `git merge` / `git push`, remind the agent of the docs gate (CLAUDE.md "Documentation").
input=$(cat)
case "$input" in
  *"git merge"*|*"git push"*)
    printf '%s' '{"hookSpecificOutput":{"hookEventName":"PreToolUse","additionalContext":"Before merging or pushing a feature: confirm its docs/specs entry, docs/user-guide entry (real captured screenshots), docs/changes entry, and TODO/BACKLOG/OPEN_QUESTIONS updates exist and match the code (docs-sync skill). If any is missing, write it first."}}'
    ;;
esac
exit 0
