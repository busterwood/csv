# Command line relational algebra

`csv.exe` is a command line tool for performing [Relational Algebra](https://en.wikipedia.org/wiki/Relational_algebra) on CSV files, written on the .NET platform.

```
csv [--help] [--in file] command [args...] [command args...]
Reads CSV from StdIn or the --in file and outputs CSV.
Command must be one of the following:
        diff[erence]    set difference between the input and other file(s)
        semijoin|exists rows from input where a matching row exists in other file(s)
        intersect       set intersection between the input and other file(s)
        join            natural join of the input and other file(s)
        project|select  removes columns from the input
        rename          changes some of the input column names
        restrict|where  removes rows from the input
        union           set union between the input and other file(s)
Non-relational commands are:
        orderby         sorts the input by one or more columns
        pretty          formats the input CSV in aligned columns
        tojson          outputs JSON array for the input CSV, one object per row
        toxml           outputs XML for the input CSV, one element per row
```

The tools only supports reading CSV at the moment, but will be extened to support reading JSON and XML.

## Examples

Join (cat) together multiple CSV files, removing the header lines of all but the first file.  All files must have the same set of columns, but column order does not matter.
Note that duplicate rows will be removed from the result.
```
csv < file1.csv union file2.csv file3.csv
```

Select only the rows from an input file where the `service` column equals `123`
```
csv < file1.csv restrict service 123
```

Multiple commands can be combined, for example, combining the first two examples, sorting the result and outputting JSON:
```
csv < file1.csv UNION file2.csv file3.csv RESTRICT service 123 ORDERBY service TOJSON
```
