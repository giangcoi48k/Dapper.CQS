using System.Data;
using System.Linq;
using System.Reflection;

namespace Dapper.CQS
{
    public abstract class CommandQuery
    {
        /// <summary>
        /// The command timeout (in seconds).
        /// </summary>
        protected virtual int? CommandTimeout => null;

        /// <summary>
        /// The type of command to execute. Default is CommandType.StoredProcedure
        /// </summary>
        protected virtual CommandType CommandType => CommandType.StoredProcedure;

        protected virtual object? GetParams()
        {
            var props = GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
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
