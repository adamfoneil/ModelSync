This is a library for generating SQL diff merge scripts, using both .NET assemblies and SQL Server databases as sources. I use this to power my [ModelSync](http://www.aosoftware.net/modelSync.html) app. Another use is to generate SQL Server tables from model classes as part of a component initialization. For example in my [WorkTracker](https://github.com/adamosoftware/WorkTracker/blob/master/WorkTracker.Library/JobManager.cs#L32) project, I create tables from a couple model classes [Job](https://github.com/adamosoftware/WorkTracker/blob/master/WorkTracker.Library/Models/Job.cs) and [Error](https://github.com/adamosoftware/WorkTracker/blob/master/WorkTracker.Library/Models/Error.cs):

```csharp
 await DataModel.CreateTablesAsync(new[]
{
    typeof(Job),
    typeof(Error)
}, GetConnection);
```
Source for the [DataModel.CreateTablesAsync](https://github.com/adamosoftware/ModelSync/blob/master/ModelSync.Library/Models/DataModel_Create.cs#L21) static method.

Nuget package: **AO.ModelSync.Library**

# Background
This is a reboot of my [SchemaSync](https://github.com/adamosoftware/SchemaSync) project, which had run into [issues](https://github.com/adamosoftware/SchemaSync/issues) I couldn't figure out.

The intent is the same: compare data models to generate a SQL diff merge script. The difference from other database diff apps out there is that this can treat .NET assemblies as data sources -- so that you can merge from C# model classes as well as to and from SQL Server databases. This enables you to use a single tool to migrate model changes end-to-end in your application.

The ultimate goal is to provide a GUI app for use as a Visual Studio "external tool" as an alternative to Entity Framework migrations. I never made peace with EF migrations, and have dabbled in alternative approaches for a long time. I believe it's a better dev experience to merge model classes on demand with a GUI tool rather than interrupting flow to write migrations. Besides being an interruption, migrations have their own administrative complications that, like I say, I never found acceptable.

That said, my approach to model sync works best with a shared development database rather than local database copies. This is because the diff scripting is not always perfect, so a little bit of manual intervention may be needed sometimes. By merging to a shared dev database, you minimize the "finesse" needed to accomplish some merges.

Things different this time around:

- No MySQL support. This will target SQL Server only for now. There are base abstract classes I can build upon later to target other platforms if I get around to it.
- No empty table rebuilds. A lot of the earlier [issues](https://github.com/adamosoftware/SchemaSync/issues) I ran into before stemmed from dropping and re-creating empty tables in order to add columns. I did that because I wanted the table column order to match the order of properties in your source files as much as possible. But a lot complexity creeps in from this use case. So, this time around there won't be any table drops and rebuilds as part of the diff algorithm. Table rebuilds will be a standalone feature you can invoke periodically -- distinct from a diff merge -- when you want to align table column order with property order.
- No seed data support initially. Maybe later.
- A smaller dependency footprint. I had a lot of type load exceptions in my old GUI app due to dependencies getting out of date with my [Postulate](https://github.com/adamosoftware/Postulate) library. The app would have one version of Postulate, while projects might have a different version. I've refactored those dependencies into a new, smaller, more stable project called [DbSchema.Attributes](https://github.com/adamosoftware/DbSchema.Attributes). I'm hoping this will clear up type load exceptions in the app as well as be more attractive to dependency-concscious devs. This is all to say that ModelSync is not pure POCO when it comes to inferring metadata like unique constraints and foreign keys. I'll have more to say about developing model classes for ModelSync compatibility.

The forthcoming GUI tool will be closed-source, but the library that powers it, this repo, will remain open source.

## In a nutshell

This example is adapted from this [test](https://github.com/adamosoftware/ModelSync/blob/master/Testing/ModelComparison.cs#L325):
```csharp
using (var cn = GetConnection())
{
    var sourceModel = await DataModel.FromAssemblyAsync(@"c:\users\adam\repos\whatever.dll");
    var destModel = await DataModel.FromSqlServerAsync(cn);
    var diff = DataModel.Compare(sourceModel, destModel);    
    string script = new SqlServerDialect().FormatScript(diff);
    Debug.Print(script);
}
```
Output might look like:
```sql
ALTER TABLE [child1] DROP CONSTRAINT [FK_child1_parentId]
GO
ALTER TABLE [child2] DROP CONSTRAINT [FK_child2_parentId]
GO
DROP TABLE [parent]
```
Source links: [DataModel.FromAssemblyAsync](https://github.com/adamosoftware/ModelSync/blob/master/ModelSync.Library/Models/DataModel.cs#L36), [DataModel.FromSqlServerAsync](https://github.com/adamosoftware/ModelSync/blob/master/ModelSync.Library/Models/DataModel.cs#L24), [DataModel.Compare](https://github.com/adamosoftware/ModelSync/blob/master/ModelSync.Library/Models/DataModel_Compare.cs#L8)
