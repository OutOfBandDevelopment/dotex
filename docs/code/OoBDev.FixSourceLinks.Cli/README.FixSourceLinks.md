# Fix - Source Links

## Summary

This tool will convert source links back to file paths.  This is useful for reporting tools that to 
not work with source links.  Otherwise you can just ensure `UseSourceLink` is set to `false` in your
`.runsettings` file.

```
/RunSettings/DataCollectionRunSettings/DataCollectors/DataCollector/Configuration/UseSourceLink = true
```