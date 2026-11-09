# oobdev-llmcodegen

Sends the files of each folder to a language model with a template and writes the responses.

## Options

Run the tool with `--help` to list its options.

```console
$ oobdev-llmcodegen --help
oobdev-llmcodegen --input <folder> --output <folder> --template <name|file> --model <model> [--provider ollama|groq-cloud] [--pattern *.*] [--overwrite true|false] [--extract true|false]
  --input      folder scanned recursively; every folder with matching files becomes one prompt (required)
  --output     folder for prompts, responses and extracted files (required)
  --template   bundled template name (for example documentation) or a Handlebars file (required)
  --model      model name sent with each prompt (required)
  --provider   ollama (default) or groq-cloud (alias groq)
  --pattern    file pattern selecting the files in a prompt (default *.*)
  --overwrite  rerun folders that already have a response (default false)
  --extract    write files found in fenced blocks of the response beside it (default true)
```
