# OoBDev Libs - Build Extensions

## Summary

.Net provides a means to extend the build for your projects though the MSBuild Project SDK
You can directly make changes to the `.csproj` files for the project or you can add additional 
files such as `Directory.Build.props` and `Directory.Build.targets` to extend the build for 
the projects in your solution.  

By using these centralized files you can improve your build process to be more consistent and 
provide better feedback for your development team while they are developing their applications.

## Notes and References

* [MSBuild Project SDK](https://learn.microsoft.com/en-us/visualstudio/msbuild/how-to-use-project-sdk?view=vs-2022)

## Additional Errors Codes

| Error Code    | Description                                                                                   |
| ------------- | --------------------------------------------------------------------------------------------- |
| OBDPK001      | The related project is marked to be packed for NUGET but does not include a package readme    |
| OBDPK002      | The package readme file identified with the project does not exist                            |