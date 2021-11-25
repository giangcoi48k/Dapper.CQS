# Dapper.CQS
A simple unit of work implementation on top of Dapper, with some basic CQS in mind.

I've found an awesome project, https://github.com/outmatic/Dapper.UnitOfWork, and I decided to extend some features so that the library can be used more easily. That's why I created this project.

For the query, I have already implemented 3 data types: single, list, paged. You just need to create a Query class, which inherits from 3 respective base classes: `QueryBase<T>`, `QueryListBase<T>`, `QueryPagedBase<T>`.

The following example is a pagination query
```c#
public class PropertyPagedFilterQuery : QueryPagedBase<Property>
{
	[Parameter]
	public string? Name { get; set; }
	protected override CommandType CommandType => CommandType.Text;
	protected override string Procedure => @"
SELECT *, COUNT(*) OVER() [COUNT] 
FROM Properties WHERE Name = @Name OR @Name IS NULL
ORDER BY [Name]
OFFSET (@page -1 ) * @pageSize ROWS
FETCH NEXT @pageSize ROWS ONLY
";

	public PropertyPagedFilterQuery(string? name, int page, int pageSize)
	{
		Name = name;
		Page = page;
		PageSize = pageSize;
	}
}
```
The default CommandType is StoredProcedure, which means you will specify the Procedure property as Stored Procedure in the database.

Take a look at specific examples in the Example project.

