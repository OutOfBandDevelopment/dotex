# OoBDev.TestUtilities


## Class: TestUtilities.TestCategories
Common test categories 

### Fields

#### Unit
Unit tests are rerun-able, standalone tests for a single operation. External resources should be mocked out so these are fast and may run within a pipeline.
#### Simulate
Simulation tests are similar to integration tests by testing the majority of the software stack. The difference being Simulations use mocked entry and persist layers so they may be executed within a pipeline without requiring external resources.
#### Integration
Integration tests should support the ability to run against deployed environments including interacting with databases and web services
#### DevLocal
Test points for local development, not expected to be safe to return and may use persisted resources

## Class: TestUtilities.TestContextExtensions
Extensions for TestContext 

### Methods


#### AddResult(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext,System.Object,System.String,System.Int32,System.String)
serialize an object to the test results for a given test run 


##### Parameters
* *context:* 
* *value:* 
* *caller:* 
* *callerLine:* 
* *callerFile:* 




##### Return value




#### AddResult(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext,System.Object,System.String@,System.String,System.Int32,System.String)
serialize an object to the test results for a given test run 


#### AddResult(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext,System.Object,System.String,System.String,System.Int32,System.String)
serialize an object to the test results for a given test run 


#### AddResult(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext,System.Object,System.String,System.String@,System.String,System.Int32,System.String)
serialize an object to the test results for a given test run 


#### AddResultFile(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext,System.String,System.Byte[])
serialize an object to the test results for a given test run 


#### AddResultFile(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext,System.String,System.Byte[],System.String@)
serialize an object to the test results for a given test run 


#### GetTestData(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext,System.Type,System.String,System.IServiceProvider)
deserialize test data from embedded resources 


##### Parameters
* *context:* 
* *type:* 
* *target:* 
* *serviceProvider:* 




##### Return value




#### GetTestDataAsync(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext,System.Type,System.String,System.IServiceProvider)
deserialize test data from embedded resources 


#### GetTestDataAsync``1(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext,System.String,System.IServiceProvider)
deserialize test data from embedded resources 


#### GetQualifiedTestName(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext)
get simplified name for executing test 


##### Parameters
* *context:* 




##### Return value




#### GetTestRunResultFiles(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext)
get path for test results folder for the executing test 


#### ResolveTestType(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext)
get current type from test context 


##### Parameters
* *testContext:* 




##### Return value




##### Exceptions

* *System.InvalidOperationException:* 




## Class: TestUtilities.TestLogger
Provides functionality for creating logger instances for testing purposes. 

### Properties

#### Factory
Gets the logger factory instance.
### Methods


#### CreateLogger``1
Creates a logger instance for the specified type. 


##### Return value
A logger instance for the specified type.

