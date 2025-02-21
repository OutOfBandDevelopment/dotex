Review the follow source code as a senior software engineer / solutions architect.   
Suggest changes to coding patterns, methods, structure and class to make the code 
more maintainable.  

## Source Files

```SearchByObjectId.json
{
  "filter": {
    "userId": {
      "eq": "6463bfae5d4d8071ae75c535"
    }
  }
}
```

```SearchQuery.json
{
  "CurrentPage": 123, //This is a 0 based page index
  "PageSize": 456, //This is a number of rows per page
  "ExcludePageCount": false, // [false | true] this will disable total page count ability
  /*
  do we want the ability to turn off paging so the response will just be the rowset without the page index info?
  */

  "SearchTerm": "search value", // this will create a, `or` search chain based on trying to match fields to

  "Filter": {
    "Propery1": "match term" // if these match search properties they will be used as predicate filters `and` chain
    /*
 
    proposal... How should we handle misses
      currently if properties exist here but are not matched in the Query Element type they are silently excluded
      should we have an option to throw an exception?  either at runtime or per query?
    */
    /*
    proposal... How should we handle nested predicates?  Should we use a dotted property path for building predicate 
    on child elements?  Something like Property1.ChildProperty?  or should predicates only apply to top level elements?

    Note, the engine already has the ability to add expression chains to handle special mappings
    */

    /* notes 
      if the value us a single value it will be an equals
      if it is a single string in the format *XYZ* the string will be contains()
      if it is a single string in the format *XYZ the string will be endswith()
      if it is a single string in the format XYZ* the string will be startswith()
    */
    /*
     MAJOR NOTE... Mongo does not have case insentive searches
    */
  },
  "OrderBy": {
    "Property2": "Descending" // [Ascending|asc|0|Descending|desc|1] 
    /*
    same proposals as above
  */
  }

  /*
    Projections?  Do we want the ability to apply a projection after the filter/sort but before the paging
  */

}

```

