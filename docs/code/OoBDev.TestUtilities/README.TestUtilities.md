# OoBDev.TestUtilities

## Summary

This is a collection of extensions that may help to improve unit tests within your projects 

## Features 

### TestContextExtensions

This class contains a set of extension methods for the TestContext class that can be used to
add test results, such as files or JSON objects, to the test results. These methods can be used 
to attach files, such as binary data or text files, to the test results, or to add JSON objects 
that can be used to analyze the test results.

### TestContext.GetTestData(...)

This method can be used to retrieve test data from embedded resources that have been included 
in the test project. The data can be retrieved as a specific type, such as a class or a JSON 
object, and can be retrieved based on the name of the test method or a specific target.

### TestContext.GetTestDataAsync(...)

This method is an asynchronous version of TestContext.GetTestData, and can be used to retrieve 
test data from embedded resources in an asynchronous way.

### TestContext.AddResult(...)

This method can be used to add a result to the test results, such as a JSON object or a file. 
The result can be added based on a specific object or a code snippet, and can be added with or 
without a specific file name.

### TestContext.AddResultFile(...)

This method can be used to add a file to the test results, based on a specific file name and 
content.

### TestContext.GetQualifiedTestName(...)

This method can be used to retrieve a simplified name for the test, which includes the fully 
qualified name of the test class and the name of the test method.

### TestContext.GetTestRunResultFiles(...)

This method can be used to retrieve a list of all the files that have been added to the test 
results for the current test run.

### TestContext.ResolveTestType(...)

This method can be used to retrieve the current type from the test context, based on the fully 
qualified name of the test class.

## TestCategories

This is intended to provide a named set of categories to provider to `TestCategoryAttribute` 
as a means to consistence within your projects to make executing filtered sets for tests easier.

