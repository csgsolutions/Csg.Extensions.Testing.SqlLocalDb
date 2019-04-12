# Introduction 
Provides utility methods for creating and managing SQL LocalDb instances.

# Using the Library
Install the NuGet Package Csg.Extensions.Testing.SqlLocalDb

Sample Code:

```csharp
	string instanceName = "Test"
	string connStr = LocalDbHelper.CreateInstance(s_instanceName);

	// do stuff with the database

    LocalDbHelper.DeleteInstance(databaseName, s_instanceName);
```

# Build and Test
Run build.ps1 and you are set.

