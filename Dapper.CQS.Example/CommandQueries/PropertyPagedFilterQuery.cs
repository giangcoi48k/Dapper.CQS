using System.Data;

namespace Dapper.CQS.Example.CommandQueries
{
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
}
