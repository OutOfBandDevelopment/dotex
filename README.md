# OoBDev - .Net Extensions

## Summary

This project contains shared libraries and examples on how to use those libraries.

## Initial Setup

No submodules are used. The all-MiniLM-L6-v2 embedding model (about 90 MB) is downloaded from Hugging Face on first use into the Hugging Face hub cache (`~/.cache/huggingface/hub`, or `HF_HUB_CACHE` / `HF_HOME`), so tests and apps that embed text need network access once. See [README.SBert.AllMiniLmL6V2.md](src/ExternalServices/SBert/OoBDev.SBert.AllMiniLmL6V2/README.SBert.AllMiniLmL6V2.md).

## Requesting a feature

Copy [docs/features/request/TEMPLATE.md](docs/features/request/TEMPLATE.md) to `docs/features/request/YYYY-MM-DD-short-name.md`, fill it in and hand it over. How requests are triaged is in [docs/features/request/README.md](docs/features/request/README.md).

## Useful Scripts

* [build.bat](.\build.bat) - build solution into [.\publish\libs](.\Publish\libs)
* [package.bat](.\package.bat) - build nuget packages into [.\publish\packages](.\publish\packages)
* [test.bat](.\test.bat) - execute unit tests and output results into [.\TestResults](.\TestResults)

