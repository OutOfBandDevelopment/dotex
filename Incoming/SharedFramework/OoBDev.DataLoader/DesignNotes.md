

## Data loader

Add app insights logging 
If it possible to use table defined functions with temporary stored procedures to generate merge scripts.
Add change detection based on modified/created dates
Make dataloader use iasyncenumerable and process is defined page sizes

What if dataloader created packages based on definitions.  The definitions would be the related dbcontext types and any source paths. The output would be a zip file with requested changes.

Extended property for version/date last loaded
Export tool to use query pattern and excludes existing references.

## Data Schema Migration

Add app insights
Use ef dbcontext script to build required data
Use dacfx 
Make schema only additive... 
   If column does not exist in model and does not have default force column to be nullable
   If column is to be deleted add delete to part pre-deploy
Extended properties for version/date created/last modified

## Notes

Use app insights internal?
Use git version or source assembly for versioning
Does dac have informational versioning?
Does dac directly support extended properties?
Schema migration and data load in one tool?


## Other Notes


    //    var sc2 = serviceCollection
    //            .AddEntityFrameworkDesignTimeServices()
    //        .AddDbContextDesignTimeServices(dbContext)
    //        ;

    //    var sp2 = sc2.BuildServiceProvider();

    //    var gen = sp2.GetRequiredService<IModel>();
    //    //var migrationBuilder = new MigrationBuilder(dbContext.Database.ProviderName);
    //    //var migrations = dbContext.Database.GetMigrations();

## Completed

Deduplicate options
   Fail
   First (implicit or defined)
   Last (implicit or defined)

## Rejected features

Pipeline output should be
(SourceProvider, SourceData, PipeLineData, Entity, Status, ValidationResults)

Status values
  Success - use as source data
  Warn - notify but exclude from source
  Error - notify and block data loading
