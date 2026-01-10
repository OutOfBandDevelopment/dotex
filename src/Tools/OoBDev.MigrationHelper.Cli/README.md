# OoBDev.MigrationHelper.Cli

Automated namespace and prefix migration tool for integrating external code into dotex.

## Purpose

This tool automates the process of migrating code from external libraries by renaming directories, files, and namespace references from a source prefix to a target prefix.

## Use Case

Designed to help migrate features from external codebases (like dotnet-libs) into the dotex framework by changing namespace prefixes.

**Example Migration:**
- Source: `OobDev.System.Abstractions` → Target: `OoBDev.System.Abstractions`
- Source: `OobDev.AspNetCore.Mvc` → Target: `OoBDev.AspNetCore.Mvc`

## How It Works

The tool performs three operations in sequence:

### 1. Rename Directories
Scans all directories recursively and renames those starting with the source prefix.

```
Before: /path/OobDev.System.Abstractions/
After:  /path/OoBDev.System.Abstractions/
```

### 2. Rename Files
Scans all files recursively and renames those starting with the source prefix.

```
Before: OobDev.System.Abstractions.csproj
After:  OoBDev.System.Abstractions.csproj
```

### 3. Replace Content
Reads all text files and replaces namespace references throughout the content.

```csharp
// Before
namespace OobDev.System.Abstractions
{
    public interface IResult { }
}

// After
namespace OoBDev.System.Abstractions
{
    public interface IResult { }
}
```

## Configuration

Edit `Program.cs` lines 7-9:

```csharp
var path = @"C:\repo\merge-em\dotex\Incomming\dotnet-lib";  // Base directory
var sourcePrefix = "SourceApp";                               // Prefix to replace
var targetPrefix = "OoBDev";                                 // New prefix
```

## Usage

### Step 1: Configure
Edit the three configuration variables in `Program.cs`:
- `path` - Root directory containing code to migrate
- `sourcePrefix` - Current namespace prefix (e.g., "SourceApp")
- `targetPrefix` - Desired namespace prefix (e.g., "OoBDev")

### Step 2: Run
```bash
dotnet run --project src/Tools/OoBDev.MigrationHelper.Cli
```

### Step 3: Verify
Review the console output showing renamed directories, files, and modified content files.

## Safety Features

- **Binary File Detection:** Skips files containing null bytes (binary files)
- **Error Handling:** Catches and reports exceptions during file processing
- **Progress Output:** Prints each renamed directory, file, and modified content file

## Example Output

```
OobDev.System.Abstractions
OobDev.AspNetCore.Mvc
OobDev.System.Abstractions.csproj
/path/to/file/IResult.cs
/path/to/file/ResponseModel/ModelResult.cs
Skip: /path/to/binary.dll
```

## Typical Migration Workflow

1. **Copy external code** to `dotex/Incomming/dotnet-lib/`
2. **Configure MigrationHelper.Cli** with path and prefixes
3. **Run the tool** to perform automated renaming
4. **Review changes** using git diff
5. **Test compilation** to ensure no broken references
6. **Move migrated code** to appropriate Framework directories
7. **Update project references** and imports
8. **Run tests** to validate integration

## Limitations

- **Simple text replacement** - Does not parse C# syntax
- **Overwrites files** - No backup created (use version control!)
- **Hardcoded configuration** - Must edit source code to change settings
- **No dry-run mode** - Changes are immediate

## Best Practices

✅ **DO:**
- Use version control (git) before running
- Test on a copy first
- Review changes with `git diff` after running
- Verify compilation after migration

❌ **DON'T:**
- Run on production code without backups
- Use on directories with uncommitted changes
- Assume it handles all edge cases
- Skip testing after migration

## Error Handling

The tool will:
- Print error messages to stderr if file operations fail
- Continue processing remaining files if one fails
- Skip binary files (containing null bytes)

## Related Tools

- **FixSourceLinks.Cli** - Corrects source links after migration
- **TemplateEngine.Cli** - Can be used for more complex transformations

## Version History

- **Current:** Simple text replacement tool
- **Future Enhancements:**
  - CLI arguments instead of hardcoded config
  - Dry-run mode
  - Backup option
  - Regex-based replacement patterns
  - C# syntax-aware renaming

## Example: SourceApp to OoBDev Migration

**Scenario:** Migrating 40 files from dotnet-libs

**Configuration:**
```csharp
var path = @"C:\repo\dotex\Incomming\dotnet-lib";
var sourcePrefix = "SourceApp";
var targetPrefix = "OoBDev";
```

**Files Affected:**
- 8 directories renamed
- 40+ .cs files renamed
- 40+ .csproj files renamed
- Namespace declarations updated in all files
- Using statements updated in all files
- XML documentation updated

**Result:** All code ready for integration into dotex Framework

---

**Status:** ✅ Production Ready - Actively used for dotnet-libs migration
