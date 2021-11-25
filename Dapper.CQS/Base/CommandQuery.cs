using System.Data;
using System.Linq;
using System.Reflection;

namespace Dapper.CQS
{
    public abstract class CommandQuery
    {
        protected virtual CommandType CommandType => CommandType.StoredProcedure;

        protected virtual object? GetParams()
        {
            var props = GetType()
                .GetProperties()
                .Select(t => (Prop: t, Attribute: t.GetCustomAttribute<ParameterAttribute>()))
                .Where(t => t.Attribute != null)
                .ToList();
            if (props.Count == 0) return null;
            var parameters = new DynamicParameters();
            foreach (var (prop, attribute) in props)
            {
                parameters.Add(attribute.Name ?? prop.Name, prop.GetValue(this, null), attribute.DbType, attribute.Direction, attribute.Size);
            }
            return parameters;
        }

        protected abstract string Procedure { get; }
    }
}
