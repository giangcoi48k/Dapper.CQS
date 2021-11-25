using System.Data;

namespace Dapper.CQS.Example.CommandQueries
{
    public class PropertyCreateCommand : CommandBase<int>
    {
        private readonly Property _property;
        protected override CommandType CommandType => CommandType.Text;
        protected override string Procedure => @"
INSERT INTO [Properties]
OUTPUT inserted.Id
VALUES(@Name, @City, @Street, @Value, @Family);

";

        public PropertyCreateCommand(Property property)
        {
            _property = property;
        }

        protected override object? GetParams()
        {
            return new
            {
                _property.Name,
                _property.City,
                _property.Street,
                _property.Value,
                _property.Family
            };
        }
    }
}
