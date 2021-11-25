using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.CQS
{
    public interface IUnitOfWork : IAutoCommitUnitOfWork
    {
        DbTransaction? Transaction { get; }
        void Commit();
        Task CommitAsync();
        Task CommitAsync(CancellationToken cancellationToken);
        void Rollback();
        Task RollbackAsync();
        Task RollbackAsync(CancellationToken cancellationToken);
    }

    public interface IAutoCommitUnitOfWork : IDisposable
    {
        DbConnection Connection { get; }
        T? Query<T>(IQuery<T> query);
        Task<T?> QueryAsync<T>(IQueryAsync<T> query);
        Task<T?> QueryAsync<T>(IQueryAsync<T> query, CancellationToken cancellationToken);
        void Execute(ICommand command);
        Task ExecuteAsync(ICommandAsync command);
        Task ExecuteAsync(ICommandAsync command, CancellationToken cancellationToken);
        T? Execute<T>(ICommand<T> command);
        Task<T?> ExecuteAsync<T>(ICommandAsync<T> command);
        Task<T?> ExecuteAsync<T>(ICommandAsync<T> command, CancellationToken cancellation);
        TRepo Repo<TRepo>() where TRepo : IRepository;
    }
}
