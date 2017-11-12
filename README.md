# Introduction
LScape.Data is a Micro-ORM with the intention of providing a simple way to map between objects and database objects.
It's intended to work with poco objects, which are either decorated with attributes, or configured through a a fluent api.

The project has expanded to include extensions to IDbConnection, IDbCommand, and IDbDataReader, to provide simpler ways
of interacting with a database from your code.

The project has been re-written as a .netstandard 2.0 library and has no other dependancies.

# Getting Started
Installation is through nuget with
```
pm> Install-Package LScape.Data
```

# Using the Mapping
At the core of LScape.Data is the `Map` class. You can create a map for any class just by creating a new Map object.
```
using LScape.Data.Mapping;

...

var map = new Map<MyClass>();
MyClass object = map.Create(dataReaderSingle);
IEnumerable<MyClass> objects = map.CreateEnumerable(dataReaderMultiple);
```

You can configure which properties of the Class take part in mapping by either decorating with the attributes
`CalculatedAttribute`, `IgnoredAttribute` and `KeyAttribute`, or using the fluent functions `Calculated`, `Ignore` and `Key`
on the `Map` class itself.

To change which column and table the class is associated with you can use the `ColumnAttribute` and `TableAttribute`, 
or directly change them in the Map with the properties.

```
[Table("MyTable")]
public class MyClass
{
    [Key]
    public int Id { get; set; }
    
    [Column("UserName")]
    public string Name { get; set; }

    public string Email { get; set; }

    [Calculated]
    public DateTime Created { get; set; }

    [Calculated]
    public DateTime? Deleted { get; set; }

    [Ignored]
    public byte[] ImageData { get; set; }
}

/// Properties can be specified by name, or linq
var map = new Map<MyClass>().Key(c => c.Id).Calculated("Created", "Deleted");
/// Manipulation

map.TableName = "MyTable";
map.Fields(f => f.PropertyName == "Name").ColumnName = "UserName";
map.Fields(f => f.PropertyName == "ImageData").FieldType = FieldType.Ignore;
```

## Mapper 
The second part of LScape.Data is the `Mapper` class, it's basically a global holder for mapped types, and holds the
default configuration for how the `Map` class behaves.

To get a `Map` from the `Mapper`:
```
var Map = Mapper.Map<T>();
```
A `Map` pulled from the mapper is cached and retains any configuration applied to it. The `PreMap` command takes
types and calculated maps upfront to save on creation time later. You can also store any Map already created
in the Mapper with `SetMap` command.

Clearing the Cached maps is through `ClearMaps`.

## Mapper Configuration
You can change the default behaviour of the Map by altering the Configuration default held by the `Mapper` class, 
or creating a new configuration and passing it into the Constructor of `Map`.

The configurations are the convention to use for table and column names from the class and properties:

Input: SomeName

Convention | Result
--- | ---
Exact | SomeName
LowerCase | somename
UpperCase | SOMENAME
SplitCase | Some_Name
SplitCaseLower | some_name
SplitCaseUpper | SOME_NAME

You can also supply delegates (Func<>) to handle the names if you need something else.
```
Mapper.Configuraton.TableNameConvert = (input) => $"tbl_{input}";
```

There are also Match functions for Ignore, Calculated and Key that take delegates that have the name and type, so you
handle general cases without having to configure for every class.

```
Mapper.Configuration.KeyMatch = (name, type) => name == "Id" && type == typeof(int);
```

# IDbConnection Extensions
There are convience extensions added to `IDbCommand` and `IDataReader` but the main ones are on `IDbConnection`.

Thery provide many ways to query the database, as well as lots of crud operations. They use the `Mapper` configured
Maps to handle opjects.

There is an Async version of each method.

Here are a few examples
```
var connection = new SqlConnection(connectionString);

...

MyClass rst = connection.ExecuteQuery<MyClass>("SELECT * FROM MyTable WHERE Id = 1");

List<MyClass> rst2 = connection.ExecuteQueryMany<MyClass>("SELECT * FROM MyTable").ToList();

MyClass rst3 = await connection.ExecuteQueryAsync<MyClass>("SELECT * FROM MyTable WHERE Id = @Id", new {Id = 1});

MyClass rst4 = connection.Get<MyClass>(new {Id = 1});

/// String values with '%' automatically cause the query to change the comparison to a like
List<MyClass> rst5 = await connection.GetAllASync<MyClass>(new {Name = "%Test%"});

int total = connection.Count<T>();

/// Insert and updates return the object as read from the database after its been saved
var savedEntity = connection.Insert(myobject);

await connection.DeleteAsync(myobject);

connection.Delete<MyClass>(new {Name = "Geoff"});
```
