using System.Data;

namespace Dapper.CQS.Example.CommandQueries
{
    public class PropertyDeleteCommand : CommandBase
    {
        [Parameter]
        public int Id { get; set; }
        protected override CommandType CommandType => CommandType.Text;
        protected override string Procedure => @"DELETE FROM [Properties] WHERE Id = @Id";

        public PropertyDeleteCommand(int id)
        {
           Id = id;
        }
    }
}
