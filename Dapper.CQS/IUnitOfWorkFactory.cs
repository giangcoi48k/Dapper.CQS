using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.CQS
{
    public interface IUnitOfWorkFactory : IDisposable
    {
        IUnitOfWork Create(bool transactional = false, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, RetryOptions? retryOptions = null);
        IUnitOfWork Create(string? connectionString, bool transactional = false, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, RetryOptions? retryOptions = null);
        Task<IUnitOfWork> CreateAsync(bool transactional = false, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, RetryOptions? retryOptions = null, CancellationToken cancellationToken = default);
        Task<IUnitOfWork> CreateAsync(string? connectionString, bool transactional = false, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, RetryOptions? retryOptions = null, CancellationToken cancellationToken = default);

        IAutoCommitUnitOfWork CreateAutoCommit(RetryOptions? retryOptions = null);
        IAutoCommitUnitOfWork CreateAutoCommit(string? connectionString, RetryOptions? retryOptions = null);
        Task<IAutoCommitUnitOfWork> CreateAutoCommitAsync(RetryOptions? retryOptions = null, CancellationToken cancellationToken = default);
        Task<IAutoCommitUnitOfWork> CreateAutoCommitAsync(string? connectionString, RetryOptions? retryOptions = null, CancellationToken cancellationToken = default);
    }
}
