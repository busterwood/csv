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
