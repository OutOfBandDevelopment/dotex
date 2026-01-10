# OoBDev - Text Templating

## Summary 

This package is used to populate text templates based on data models.

## IGenerateText

Text template engine

### Task<string> GenerateAsync<TModel>(string template, TModel model)

Text template engine based on HTML and JSON Paths.  see Readme.IGenerateText.md for examples.
The input `template` uses a combination of HTML, JSON Paths, and custom extensions to create 
textual content based on the input `model`

## ITemplateResolver

text resolver allows to lookup individual templates as well as list 
all existing templates

### Task<string> GetTemplateAsync(string templateName, CultureInfo culture)

Lookup text template by name and culture (language and country)

### Task<IEnumerable<TemplateSummaryModel>> GetTemplateSummariesAsync()

return a list of current templates

## ITextTemplateProvider

Persistence provider for text templates

### Task<string> GetAsync(string name, string language, string country)

lookup a text template by name, language and country

### Task<IEnumerable<TemplateSummaryModel>> GetSummaryAsync()

list all templates

### IQueryable<TextTemplateModel> Query()

allow composable queries for existing templates

### Task<Guid> SaveAsync(TextTemplateModel model)

store template changes