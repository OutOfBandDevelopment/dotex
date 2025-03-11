

 - https://www.jhipster.tech/jdl/entities-fields/
  
 ## Handlebars script to convert DB Json to JDL

 ```hbs
 /*
    Example Stuff
*/

{{#each this}}
application {
  config {
    baseName {{SchemaName}}
    applicationType monolith
    authenticationType jwt
    prodDatabaseType mssql
  }
  entities *
}

{{#each objects}}
entity {{CleanObjectName}} {
{{#each columns}}
    {{CleanColumnName}} {{#each types}}{{JDL_TypeDescription}}{{/each}}
{{/each}}
}

{{/each}}

{{/each}}
 ```