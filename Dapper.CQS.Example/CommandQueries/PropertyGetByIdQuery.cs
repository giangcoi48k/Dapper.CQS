using System.Data;

namespace Dapper.CQS.Example.CommandQueries
{
    public class PropertyGetByIdQuery : QueryBase<Property>
    {
        [Parameter]
        public int Id { get; set; }
        protected override CommandType CommandType => CommandType.Text;
        protected override string Procedure => "SELECT * FROM Properties WHERE Id = @Id";

        public PropertyGetByIdQuery(int id)
        {
            Id = id;
        }
    }
}
