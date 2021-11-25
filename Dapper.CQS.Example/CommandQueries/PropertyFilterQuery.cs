using System.Data;

namespace Dapper.CQS.Example.CommandQueries
{
    public class PropertyFilterQuery : QueryListBase<Property>
    {
        [Parameter]
        public string? Name { get; set; }
        protected override CommandType CommandType => CommandType.Text;
        protected override string Procedure => "SELECT * FROM Properties WHERE Name = @Name OR @Name IS NULL";

        public PropertyFilterQuery(string? name)
        {
            Name = name;
        }
    }
}
