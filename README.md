A data schema should be a thing that is defined once and used everywhere, for example:

1. create a schema in a databse
2. create a table for a schema
3. used as parameters and result type of a stored procedure
4. used as the parameter to insert, update, or merge (upsert)
3. used as a datatype in client software (e.g. Java, C#)
4. used as when sending and receiving messages (e.g. JMS, RabbitMQ, Tibco RV)
5. used when converting to and from other format, e.g. JSON, XML, CSV

A schema should also support:

* evolving, i.e. adding fields over time, (depricating fields?)
* extension, i.e. schema A extends schema B
* duck typing, i.e. if a schema has the same column names and types then it is the same, regardless of the order of columns

# Command line relational algebra

`csv.exe` is a command line tool for performing [Relational Algebra](https://en.wikipedia.org/wiki/Relational_algebra) on CSV files, written on the .NET platform.

```
csv command [--help] [--in file] [args...]
Reads CSV from StdIn or the --in file and outputs CSV.
Command must be one of the following:
        diff[erence]    set difference between the input and other file(s)
        semijoin|exists rows from input where a matching row exists in other file(s)
        intersect       set intersection between the input and other file(s)
        join            natural join of the input and other file(s)
        orderby         sorts the input by one or more columns
        project|select  removes columns from the input
        rename          changes some of the input column names
        restrict|where  removes rows from the input
        union           set union between the input and other file(s)
```

The tools only support CSV at the moment, but will be extened to support JSON and XML.

## csv.exe difference

```
c:\Dev\BusterWood.Data>csv diff --help
csv diff[erence] [--all] [--in file] [--rev] [file ...]
Outputs the rows in the input CSV that do not appear in any of the additional files
        --all    do NOT remove duplicates from the result
        --in     read the input from a file path (rather than standard input)
        --rev    reverse the difference
```

## csv.exe semijoin (exists)

```
csv exists [--in file] [file ...]
Outputs the input CSV where only if a row exists in the additional input files with matching values in common columns.
        --in     read the input from a file path (rather than standard input)
```

## csv.exe intersect

```
csv intersect [--all] [--in file] [file ...]
Outputs the set intersection of the input CSV and some additional files
        --all    do NOT remove duplicates from the result
        --in     read the input from a file path (rather than standard input)
```

## csv.exe join

```
csv join [--in file] [file ...]
Outputs the natural join of the input CSV and some additional files based on common columns
        --in     read the input from a file path (rather than standard input)
```

## csv.exe orderby

```
csv orderby [--all] [--in file] Column [Column ...]
Sorts the input CSV by one or more columns
        --all  do NOT remove duplicates from the result
        --in   read the input from a file path (rather than standard input)
```

## csv.exe project (select)

```
csv project [--all] [--in file] [--away] Column [Column ...]
Outputs in the input CSV with only the specified columns
        --all   do NOT remove duplicates from the result
        --in    read the input from a file path (rather than standard input)
        --away  removes the input columns from the source
```

## csv.exe rename

```
csv rename [--in file] [old new...]
Outputs the input CSV chaning the name of one or more columns.
        --all    do NOT remove duplicates from the result
        --in     read the input from a file path (rather than standard input)
```

## csv.exe restrict

```
csv restrict [--all] [--in file] [--away] [--equal] Column Value [Column Value ...]
Outputs rows of the input CSV where Column equals the string Value.  Multiple tests are supported.
        --all       do NOT remove duplicates from the result
        --in        read the input from a file path (rather than standard input)
        --away      removes rows from the input that match the test(s)
        --contains  changes the test to be Column contains Value, rather that equality
```

## csv.exe union

```
csv union [--all] [--in file] [file ...]
Outputs the set union of the input CSV and some additional files
        --all    do NOT remove duplicates from the result
        --in     read the input from a file path (rather than standard input)
```