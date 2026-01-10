# Template Editor

## Table of Contents

 * [Summary](#summary)
 * [Special Replacement Tags](#special-replacement-tags)
   * [value-of](#value-of)
   * [value-attr](#value-attr)
   * [repeater](#repeater)
   * [condition](#condition)
 * [Addendum](#addendum)
   * [Sample data for examples](#sample-data-for-examples)
   * [Requirements](#requirements)

## Summary

The text template engine for communication center is based around html using the HtmlAgilityPack.

## Special Replacement Tags

### value-of

The `value-of` is used for value replacement.  The *binding* attribute is used to 
provider the [JSON path][JsonNetPath] from the source data object.

`value-of` has optional attributes `format` and `type`.  By default format works for date/time values.
If you also the type attribute you may choose between formatting `DATE` (date/time) and `Number` decimal)
If the value maybe converted to the related type the formatter string will be used as a standard .Net 
formatter for the matched type values.

#### Template
```
To: <value-of binding="$.Parent.FirstName" /> <value-of binding="$.Parent.LastName" />
```

#### Result
```
To: Chris Watson
```

#### Template (with format)
```
<value-of binding="$.Class.Date" format="MM/dd/yyyy" />
```

#### Result (with format)
```
05/12/2020
```

#### Template (with format and type)
```
<value-of binding="$.FullSchedule[2].Period" format="0.00" type="NUMBER" />
```

#### Result (with format and type)
```
3.00
```

### value-attr

The `value-attr` is used to add or replace an attribute on the containing element.  The *binding* 
attribute is used to provider the [JSON path][JsonNetPath] from the source data object.  The 

#### Template

```
<a alt="replace it" href="replace me!"><value-attr binding="$.Link.Url" item="href"  />Link Desc</a>
```

#### Result

```
<a alt="replace it" href="http://learnmark.co">Link Desc</a>
```

### repeater

The `repeater` is used present a set of data.  The *binding* attribute is used to provider the 
[JSON path][JsonNetPath] from the source data object.  The body contained within the repeater tag is 
used as a template or the child objects.

#### Text Template

```
<repeater binding="FullSchedule"><value-of binding="Name" />   <value-of binding="Period" />
</repeater>
```

#### Result

```
Science 101   1
Math 102   2
English 101   3

```

#### Filted Data Template

```
<repeater binding="FullSchedule[?(@.Period >= 2)]"><value-of binding="Name" />   <value-of binding="Period" />
</repeater>
```

#### Result

```
Math 102   2
English 101   3

```

#### HTML Template

```HTML
<table>
    <thead>
        <tr>
            <th>Class</th>
            <th>Period</th>
        </tr>
    </thead>
    <tbody>
        <repeater binding="FullSchedule">
            <tr>
                <td><value-of binding="Name" /></td>
                <td><value-of binding="Period" /></td>
            </tr>
        </repeater>
    </tbody>
</table>
```

#### Result

```HTML
<table>
    <thead>
        <tr>
            <th>Class</th>
            <th>Period</th>
        </tr>
    </thead>
    <tbody>
        
            <tr>
                <td>Science 101</td>
                <td>1</td>
            </tr>
        
            <tr>
                <td>Math 102</td>
                <td>2</td>
            </tr>
        
            <tr>
                <td>English 101</td>
                <td>3</td>
            </tr>
        
    </tbody>
</table>
```

### condition

This extension is used to make a condition block.  The contained template will only output if the value 
in the attribute `rule` is true.

#### Text Template

```
<repeater binding="FullSchedule"><condition rule="@.Period == 2">*</condition><value-of binding="Name" />   <value-of binding="Period" />
</repeater>
```

#### Result

```
Science 101   1
*Math 102   2
English 101   3

```

### data-binding (extension attribute)

`data-binding` attribute may be placed on any existing element.  This binding will replace the inner content with the binding results. 

#### Template
```
<div data-binding=""$.Parent.FirstName"">Other Stuff</div>
```

#### Result
```
<div>Chris</div>
```

### Scoping (item/source attributes)

Using the `item` and `source` attributes you may extend other template elements.  `item` allows you to 
declared a named scope.  `source` allows you to target that scope.  You may also use `@` as a binding value 
to use the current data element and `$` to select from root.   

#### Template
```
<repeater binding="Periods" item="period"><repeater binding="Things" item="thing">
    <value-of source="period" binding="Number" /><value-of binding="$.Marker" /> <value-of source="thing" binding="@" /></repeater></repeater>
```

#### Result
```

    1) Math
    1) English
    1) Other
    2) History
    2) Science
    3) Speech
    3) Band
```

## Addendum

### Sample data for examples 

```JSON
{
  "Parent": {
    "FirstName": "Chris",
    "LastName": "Watson"
  },
  "Student": {
    "FirstName": "Johnnie",
    "LastName": "Watson"
  },
  "Class": {
    "Name": "English 101",
    "Date": "5/12/2020"
  },
  "Link": {
    "Url": "http://learnmark.co",
    "Title": "LearnMark"
  },
  "FullSchedule": [
    {
      "Name": "Science 101",
      "Period": 1
    },
    {
      "Name": "Math 102",
      "Period": 2
    },
    {
      "Name": "English 101",
      "Period": 3
    }
  ],
  "Marker": ")",
  "Periods": [
    {
      "Number": 1,
      "Things": [ "Math", "English", "Other" ]
    },
    {
      "Number": 2,
      "Things": [ "History", "Science" ]
    },
    {
      "Number": 3,
      "Things": [ "Speech", "Band" ]
    }
  ]
}
```

## Requirements

This application requires access to an instance of SQL Server.  If you do not hae one installed 
you may download [SQL LocalDB][SQLLocalDBMSI].  To Update the database schema you will also need
SqlPackage.exe that is part of the [DacFramework][DacFrameworkMSI]

## External Links

[JsonNetPath]:https://www.newtonsoft.com/json/help/html/QueryJsonSelectTokenJsonPath.htm
[SQLLocalDBMSI]:https://download.microsoft.com/download/7/c/1/7c14e92e-bdcb-4f89-b7cf-93543e7112d1/SqlLocalDB.msi
[DacFrameworkMSI]:https://docs.microsoft.com/en-us/sql/tools/sqlpackage-download?view=sql-server-ver15
