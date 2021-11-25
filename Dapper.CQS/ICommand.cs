using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.CQS
{
    public interface ITransactionRequired
    {
        bool TransactionRequired { get; }
    }

    public interface ICommand : ITransactionRequired
    {
        void Execute(IDbConnection connection, IDbTransaction? transaction);
    }

    public interface ICommand<out T> : ITransactionRequired
    {
        T? Execute(IDbConnection connection, IDbTransaction? transaction);
    }

    public interface ICommandAsync : ITransactionRequired
    {
        Task ExecuteAsync(IDbConnection connection, IDbTransaction? transaction, CancellationToken cancellationToken = default);
    }

    public interface ICommandAsync<T> : ITransactionRequired
    {
        Task<T?> ExecuteAsync(IDbConnection connection, IDbTransaction? transaction, CancellationToken cancellationToken = default);
    }
}
