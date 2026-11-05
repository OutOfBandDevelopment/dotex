# oobdev-xslt

Applies an XSLT template to XML, HTML, JSON, YAML, C#, VB, MSBuild structured logs, DACPACs or paths.

## Options

Run the tool with `--help` to list its options.

```console
$ oobdev-xslt --help
oobdev-xslt -i <input> -t <template.xslt> -o <output> [-e <exclude>] [-x <input-type>] [-s <sandbox>] [-m]
  -i, --input       input file or wildcard
  -t, --template    XSLT file
  -o, --output      output file or folder
  -e, --exclude     wildcard of inputs to skip
  -x, --input-type  Xml (default), Html, Json, Yaml, CSharp, VB, MSBuildStructuredLog, DacPac, Path, ByExtension
  -s, --sandbox     folder the template may read and write (default: the folder of the output)
  -m, --merge       merge all inputs into one document
```
