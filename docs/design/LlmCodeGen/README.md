# LlmCodeGen

**Status:** Design · **Owner decision:** consolidate the two BulkLlm tools (Q12, answered 2026-10-09)

A single command-line tool, `OoBDev.LlmCodeGen.Cli`, that walks a source tree, builds one prompt per folder from a template, sends it to a configured LLM provider and saves the response. It replaces the two retired prototypes `BulkLlm.GroqNet.Cli` and `BulkLlm.Ollama.Cli`, which were 95% identical and differed only in the LLM client (removed in `438b2885`, recoverable from git history).

## Documents

| Document | Contents |
|----------|----------|
| [Requirements](requirements.md) | Goals, non-goals, what the prototypes did and lacked |
| [Architecture](architecture.md) | Components, reuse of existing framework pieces, flow |
| [API design](api-design.md) | Options, commands, response file format |
| [Testing strategy](testing-strategy.md) | Unit and Simulate tests, live checks |

## Decision summary

- No new provider abstraction: the existing `IMessageCompletion` (model name plus prompt in, text out) selected through `TryAddConfiguredKeyedService<T>` already covers Ollama and Groq.
- Prompts are rendered with Handlebars.Net through a small `PromptRenderer` (the framework template engine is tied to template contexts and sources that a prompt does not need).
- The disabled file extraction in both prototypes is implemented as a tested, pure parser.
- Paths, endpoint and model move from hard-coded values to options validated with data annotations.
