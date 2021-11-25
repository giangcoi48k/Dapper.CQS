using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.CQS.Internal
{
    internal class AutoCommitUnitOfWork : IAutoCommitUnitOfWork
    {
        public DbConnection Connection { get; }
        public DbTransaction? Transaction { get; set; }

        private readonly IServiceProvider _serviceProvider;
        private readonly RetryOptions? _retryOptions;
        private readonly CancellationToken _cancellationToken;
        private bool _disposed;

        internal AutoCommitUnitOfWork(
            IServiceProvider serviceProvider,
            DbConnection connection,
            RetryOptions? retryOptions = null,
            CancellationToken cancellationToken = default)
        {
            _serviceProvider = serviceProvider;
            _retryOptions = retryOptions;
            _cancellationToken = cancellationToken;
            Connection = connection;
        }

        public T? Query<T>(IQuery<T> query)
        {
            return Retry.Do(() => query.Query(Connection, Transaction), _retryOptions);
        }

        public Task<T?> QueryAsync<T>(IQueryAsync<T> query)
        {
            return Retry.DoAsync(() => query.QueryAsync(Connection, Transaction, _cancellationToken), _retryOptions);
        }

        public Task<T?> QueryAsync<T>(IQueryAsync<T> query, CancellationToken cancellationToken)
        {
            return Retry.DoAsync(() => query.QueryAsync(Connection, Transaction, cancellationToken), _retryOptions);
        }

        public void Execute(ICommand command)
        {
            TransactionRequired(command);
            Retry.Do(() => command.Execute(Connection, Transaction), _retryOptions);
        }

        public T? Execute<T>(ICommand<T> command)
        {
            TransactionRequired(command);
            return Retry.Do(() => command.Execute(Connection, Transaction), _retryOptions);
        }

        public Task ExecuteAsync(ICommandAsync command) => ExecuteAsync(command, _cancellationToken);

        public Task ExecuteAsync(ICommandAsync command, CancellationToken cancellationToken)
        {
            TransactionRequired(command);
            return Retry.DoAsync(() => command.ExecuteAsync(Connection, Transaction, cancellationToken), _retryOptions);
        }

        public Task<T?> ExecuteAsync<T>(ICommandAsync<T> command) => ExecuteAsync(command, _cancellationToken);

        public Task<T?> ExecuteAsync<T>(ICommandAsync<T> command, CancellationToken cancellationToken)
        {
            TransactionRequired(command);
            return Retry.DoAsync(() => command.ExecuteAsync(Connection, Transaction, cancellationToken), _retryOptions);
        }

        public TRepo Repo<TRepo>() where TRepo : IRepository
        {
            return _serviceProvider.GetRequiredService<TRepo>();
        }

        protected virtual bool TransactionRequired(ITransactionRequired command)
        {
            return false;
        }

        #region Finalizers
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AutoCommitUnitOfWork() => Dispose(false);

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Transaction?.Dispose();
                Connection?.Dispose();
            }
            _disposed = true;
        }

        #endregion Finalizers
    }

    internal class UnitOfWork : AutoCommitUnitOfWork, IUnitOfWork
    {
        private readonly CancellationToken _cancellationToken;

        internal UnitOfWork(
            IServiceProvider serviceProvider,
            DbConnection connection,
            bool transactional = false,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            RetryOptions? retryOptions = null,
            CancellationToken cancellationToken = default
        ) : base(serviceProvider, connection, retryOptions, cancellationToken)
        {
            _cancellationToken = cancellationToken;
            if (transactional)
                Transaction = connection.BeginTransaction(isolationLevel);
        }

        public void Commit() => Transaction?.Commit();

        public async Task CommitAsync()
        {
            if (Transaction != null)
            {
                await Transaction.CommitAsync(_cancellationToken);
            }
        }

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            if (Transaction != null)
            {
                await Transaction.CommitAsync(cancellationToken);
            }
        }

        public void Rollback() => Transaction?.Rollback();

        public async Task RollbackAsync()
        {
            if (Transaction != null)
            {
                await Transaction.RollbackAsync(_cancellationToken);
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            if (Transaction != null)
            {
                await Transaction.RollbackAsync(cancellationToken);
            }
        }

        protected override bool TransactionRequired(ITransactionRequired command)
        {
            if (command.TransactionRequired && Transaction == null)
                throw new Exception($"The command {command.GetType()} requires a transaction");
            return true;
        }
    }
}
