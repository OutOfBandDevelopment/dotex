
        
## Documentation To do

- [ ] Write up how to use and configure these things
  - [ ] Queuing System
    - [ ] Sender
    - [ ] Receiver
  - [ ] Email
    - [ ] Sender
  - [x] Templating Engine
    - [x] Handlebars
    - [x] XSLT
        
- [ ] Write up docs on 
  - [ ] Packages
  - [ ] Core projects
  - [ ] Dev projects
    
  - [ ] Extensible framework with patterns and practices 
  - [ ] Agnostic, queue, platform agnostic
  - [ ] Centralize openai chatbot

## General Notes

* https://dev.to/bhaeussermann/adding-a-swagger-ui-page-to-your-express-rest-api-3cbc
* https://stackoverflow.com/questions/62950730/springdoc-openapi-ui-oauth-2-0-authorization-code-flow-with-pkce
* https://stackoverflow.com/questions/74614369/how-to-run-swagger-3-on-spring-boot-3
* https://swagger.io/tools/swaggerhub/
* https://github.com/docker/for-win/issues/868
* https://learn.microsoft.com/en-us/aspnet/core/test/middleware?view=aspnetcore-8.0
   
* [Comparing Container Apps with other Azure container options | Microsoft Learn](https://learn.microsoft.com/en-us/azure/container-apps/compare-options)
* [Vector Index Basics and the Inverted File Index - Zilliz Vector database blog](https://zilliz.com/learn/vector-index)
* https://github.com/microsoft/DacFx/blob/main/src/Microsoft.Build.Sql/docs/Functionality.md
* https://github.com/microsoft/sql-server-language-extensions/tree/main/language-extensions/dotnet-core-CSharp
* [c# - Accessing IOptions and DbContext within ConfigureServices - Stack Overflow](https://stackoverflow.com/questions/61783171/accessing-ioptions-and-dbcontext-within-configureservices)
* Notes
  *  And add health checks
  *  Should we add a signing key for events so we can validate impersonation?
  *  Should we support impersonation for back end events
  *  Should we add weighted predicates in the filter model
  *  Can the filter model be ported to java
* Add support for shopping carts
  * https://www.shopify.com/
  
## JS/TS Notes

- [ ] How to run tagged tests
- [ ] Hot to get coverage reports
- [ ] Coverlet?

* https://github.com/coverlet-coverage/coverlet
* https://github.com/cobertura/cobertura
* https://stackoverflow.com/questions/34299647/cobertura-with-javascript
* https://karma-runner.github.io/0.8/config/coverage.html
* https://theintern.io/docs.html#Intern/4/docs/README.md
* https://istanbul.js.org/docs/tutorials/mocha/

* JS Docs
  * https://jsdoc.app/about-commandline
  * https://www.npmjs.com/package/jsdoc
  * https://github.com/gajus/eslint-plugin-jsdoc
  
## AI Notes

https://azure.microsoft.com/en-us/products/ai-services/ai-bot-service
https://openai.com/
https://azure.microsoft.com/en-us/products/copilot
https://www.howtogeek.com/build-a-personal-ai-assistant-with-chatgpts-custom-instructions/
https://www.nuget.org/packages/Azure.AI.OpenAI/
https://www.pinecone.io/learn/vector-embeddings/
https://medium.com/@hermanschutte/how-to-custom-train-and-fine-tune-models-with-the-chatgpt-api-afb796aaf2fe

## Idea List
* UI
  * MAUI
  * Blazor
  * Vue 
  * React 
  * Angular
  * Flutter
  * Native
    * React Native
  * Toolkits
    * Material
    * PrimeNG
            
* Backend
  * Dot Net
  * Java
  * Node
  * Python
  * Go-long 
  * Rust
* Persistence
  * Document Store
    * Mongo
  * Relational
    * SQL
    * Oracle
    * MySql
  * Big Data
    * Snowflake
    * Hadoop
            
* Traceability/analytics
  * Metrics
  * Logging
  * Tracing
  * Tracking
    * User Context

* Query Patterns
  * Odata
  * Graphql
  * Matt's magical framework
    * Reactional/masking
  * Builder patterns?
    * Direct query
    * JPA 2?
* Testing
  * Data Generation
* Domain Specific Language
* IOT 
  * Events Driven Development

* Code Generation
  * OpenAPI/Swagger

* queue
* access level redaction
* communication central
  
    * Logging System
      * Open Telemetry 
      * Application Insights
      * Google Analytics
      * Splunk
      * AWS Logging?
    * Integrations
      * Templating
      * Form Builder
    * ETL 
      * SSIS
      * Azure Data Factory
      * MFT
        * ?
* Data Conversion

    * Database
      * Snowflake?
    * Queue
      * Kafka
      * RabbitMQ
      * Service Bus
    * Application Gateways
      * …
    * Clustering
      * Kubernetes
      * Docker
      * Containerd
    * Run time frameworks
      * DAPR
        
    * Global Search
      * Application level like Elastic
      * Natural Language
    * Machine Learning / Artificial Intelligence
        
    * Auth Stores
      * B2C
      * Keycloak

* Confugurator
  * Custom club builder
* Maps

* form builder
  * https://github.com/kelp404/angular-form-builder
  * https://form.io/open-source
  * https://github.com/formkiq/formkiq-core
* document management
  * https://github.com/hmcts/document-management-client
  * https://github.com/TheNachi/Document-Management-System
  * https://github.com/christopherthielen/check-peer-dependencies

## Old setup notes

Install SQL Server 2019 SqlLocalDb 
    https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver15
Install DACFramework/SQLPackage.exe 
    https://docs.microsoft.com/en-us/sql/tools/sqlpackage/sqlpackage-download?view=sql-server-ver15
Install Visual Studio 2022 perfered but 2019 will work
Install az-lazy from nuget
    dotnet tool install --global az-lazy --version 1.3.1

load the solution in visual studio
build the entire solution (mainly dataloader and exampledb)
start the OoBDev.Example.Functions project (this will start storage emulator for VS2019 or Azurite for VS2022)

from a command prompt run the following...
    SetupLocalStorageEmulator.bat
    SetupLocalDb.bat

if you have issues with database schema you can delete the sqllocaldb by running the "RemoveLocalDb.bat"

After this is all completed you can run the OoBDev.Example.WebAPI with the "with B2C" profile if you are using local development
If you need to test the azure functions such as complex events, emails to scheduled jobs you can also run the OoBDev.Example.Functions project


## Suggested DevTools

* For .Net
  * Visual Studio Pro or Enterprise 2022+
  * Visual Studio Code
    * https://marketplace.visualstudio.com/items?itemName=yzhang.markdown-all-in-one
    * https://marketplace.visualstudio.com/items?itemName=yzane.markdown-pdf
    * https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-wsl
    * https://marketplace.visualstudio.com/items?itemName=jebbs.plantuml            
  * Node LTS 
  * SQL Server Management Studio
  * Git for Windows
  * Git LFS
* For Java
  * IntelliJ
* Optional
  * MongoDB with Compass
  * SQL Server LocalDB
  * Docker Desktop Installer 
  * Beyond Compare
  * OBS Studio
* Azure Storage Explorer

* State machines
* Model validation 
* Data authorization
* Reports 
