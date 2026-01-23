# OoBDev - .Net Extensions

## Summary

This project contains shared libraries and examples on how to use those libraries.

## Initial Setup

⚠️ **IMPORTANT:** This repository uses git submodules for ML models (SBert All-MiniLM-L6-v2).

**If tests fail with missing model files**, you need to initialize submodules first.

After cloning, initialize submodules:

**Linux/macOS:**
```bash
./scripts/init-submodules.sh
```

**Windows:**
```bat
scripts\init-submodules.bat
```

Or manually:
```bash
git submodule update --init --recursive
git submodule foreach --recursive 'git lfs pull'
```

**For future clones**, use:
```bash
git clone --recurse-submodules <repository-url>
```

Or configure git globally to automatically handle submodules:
```bash
git config --global submodule.recurse true
```

### What gets initialized:
- `src/ExternalServices/SBert/OoBDev.SBert.AllMiniLML6v2Sharp/model/` - ONNX model files (~90MB)
  - `model.onnx` - The actual ML model (Git LFS)
  - `vocab.txt` - Tokenizer vocabulary

## Useful Scripts

* [build.bat](.\build.bat) - build solution into [.\publish\libs](.\Publish\libs)
* [package.bat](.\package.bat) - build nuget packages into [.\publish\packages](.\publish\packages)
* [test.bat](.\test.bat) - execute unit tests and output results into [.\TestResults](.\TestResults)

