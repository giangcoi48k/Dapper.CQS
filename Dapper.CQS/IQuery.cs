using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.CQS
{
    public interface IQuery<out T>
    {
        T? Query(IDbConnection connection, IDbTransaction? transaction);
    }

    public interface IQueryAsync<T>
    {
        Task<T?> QueryAsync(IDbConnection connection, IDbTransaction? transaction, CancellationToken cancellationToken = default);
    }
}
