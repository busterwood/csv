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
        semijoin|exists only row from input where a matching row exists in other file(s)
        intersect       set intersection between the input and other file(s)
        join            natural join of the input and other file(s)
        orderby         sorts the input by one or more columns
        project|select  removes columns from the input
        rename          changes some of the input column names
        restrict|where  removes rows from the input
        union           set union between the input and other file(s)
```

The tools only support CSV at the moment, but will be extened to support JSON and XML.