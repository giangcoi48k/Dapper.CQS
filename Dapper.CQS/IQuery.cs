using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.CQS
{
    public interface IQuery<out T> : ITransactionRequired
    {
        T? Query(IDbConnection connection, IDbTransaction? transaction);
    }

    public interface IQueryAsync<T> : ITransactionRequired
    {
        Task<T?> QueryAsync(IDbConnection connection, IDbTransaction? transaction, CancellationToken cancellationToken = default);
    }
}
