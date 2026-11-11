# oobdev-dacpac

Builds a DACPAC from a SQL CLR assembly.

## Options

Run the tool with `--help` to list its options.

```console
$ oobdev-dacpac --help
oobdev-dacpac - Builds a DACPAC from a SQL CLR assembly.
Pass options as --name value (a boolean needs an explicit true or false).
  --tool     <Unknown|SqlClr|Merge>
  --version  <string>
  --project  <string>
  --dacpac   <string>
  --pdb      <string>
  --sqlclr   <string> (required)


dacpactools --tool Merge --target-path <file> [options]

Merges several DACPACs into one package.

  -t, --template <file>        Template file read through the object converter (JSON); options below override it.
  -s, --source-path <folder>   Folder the source patterns are resolved against (default: current folder).
  -p, --source-patterns <list> Semicolon separated glob patterns (default: *.dacpac); a leading ! excludes.
  -r, --target-path <file>     The merged DACPAC to write.
  -n, --name <text>            Package name (default: the target file name).
  -d, --description <text>     Package description (default: the local time).
  -v, --version <text>         Package version (default: 0.0.0).
  -b, --build-version <text>   Build version written by the post-deployment script (default: the version).
      --setting-modeloptionsource <Custom|First|Last>
                               Where the model options come from.
```
