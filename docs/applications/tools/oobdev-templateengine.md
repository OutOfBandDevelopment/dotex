# oobdev-templateengine

Renders a Handlebars or XSLT template over input files.

## Options

Run the tool with `--help` to list its options.

```console
$ oobdev-templateengine --help
oobdev-templateengine - Renders a Handlebars or XSLT template over input files.
Pass options as --name value (a boolean needs an explicit true or false).
  --input                   <string>
  --Template                <string> (required)
  --input-type              <Unknown|Xml|Html|Json|Yaml|CSharp|VB|MSBuildStructuredLog|DacPac|Path|ByExtension>
  --output                  <string>
  --exclude                 <string>
  --sandbox                 <string>
  --merge                   <true|false>
  --file-template-path      <string>
  --file-template-sandbox   <string>
  --file-template-priority  <int32>
```
