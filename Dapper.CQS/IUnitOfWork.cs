using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.CQS
{
    public interface IUnitOfWork : IDisposable
    {
        DbConnection Connection { get; }
        DbTransaction? Transaction { get; }
        void Commit();
        Task CommitAsync();
        Task CommitAsync(CancellationToken cancellationToken);
        void Rollback();
        Task RollbackAsync();
        Task RollbackAsync(CancellationToken cancellationToken);
        T? Query<T>(IQuery<T> query);
        Task<T?> QueryAsync<T>(IQueryAsync<T> query);
        Task<T?> QueryAsync<T>(IQueryAsync<T> query, CancellationToken cancellationToken);
        void Execute(ICommand command);
        Task ExecuteAsync(ICommandAsync command);
        Task ExecuteAsync(ICommandAsync command, CancellationToken cancellationToken);
        T? Execute<T>(ICommand<T> command);
        Task<T?> ExecuteAsync<T>(ICommandAsync<T> command);
        Task<T?> ExecuteAsync<T>(ICommandAsync<T> command, CancellationToken cancellation);
    }
}
