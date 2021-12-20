using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.CQS
{
    public abstract class CommandBase : CommandQuery, ICommand, ICommandAsync
    {
        public virtual bool TransactionRequired => true;

        public virtual void Execute(IDbConnection connection, IDbTransaction? transaction)
        {
            connection.Execute(Procedure, GetParams(), TransactionRequired ? transaction : null, CommandTimeout, CommandType);
        }

        public virtual async Task ExecuteAsync(IDbConnection connection, IDbTransaction? transaction, CancellationToken cancellationToken = default)
        {
            await connection.ExecuteAsync(Procedure, GetParams(), TransactionRequired ? transaction : null, CommandTimeout, CommandType);
        }
    }

    public abstract class CommandBase<T> : CommandQuery, ICommand<T>, ICommandAsync<T>
    {
        public virtual bool TransactionRequired => true;

        public virtual T? Execute(IDbConnection connection, IDbTransaction? transaction)
        {
            return connection.ExecuteScalar<T>(Procedure, GetParams(), TransactionRequired ? transaction : null, CommandTimeout, CommandType);
        }

        public virtual async Task<T?> ExecuteAsync(IDbConnection connection, IDbTransaction? transaction, CancellationToken cancellationToken = default)
        {
            return await connection.ExecuteScalarAsync<T>(Procedure, GetParams(), TransactionRequired ? transaction : null, CommandTimeout, CommandType);
        }
    }
}
