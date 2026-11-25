# oobdev-packman

Maintains the central package versions in `Directory.Packages.props`.

## Options

Run the tool with `--help` to list its options.

```console
$ oobdev-packman --help
oobdev-packman [-s <Directory.Packages.props>] [-o <output>] [-n] [--no-nuget] [-t <Name,1.2.3;...>] [--nuget-url <url>]
               [--nuget-feed-type <type>] [--get-properties] [--follow-imports] [-g <property>] [-v]
               [--update-projects --projects <glob;!glob> --projects-path <folder>]
  -s, --source          props file (default: Directory.Packages.props in the current folder)
  -o, --output          save here instead of over the source
  -n, --no-save         do not save changes
      --no-nuget        do not look up newer versions
  -t, --target-versions set property versions (Name,1.2.3;Other,2.0.0)
      --nuget-url       NuGet service index (default https://api.nuget.org/v3/index.json)
      --nuget-feed-type HttpV3 (default) or HttpV2
      --get-properties  print the properties of the props file
      --follow-imports  replace Import elements with the imported content
  -g, --get-property    print one property and stop
  -v, --get-version     print the tool version and stop
      --update-projects set PackageReference versions of the matching projects from the props file
      --projects        project globs separated by ';', a leading '!' excludes
      --projects-path   folder the globs are relative to (default: current folder)
```
