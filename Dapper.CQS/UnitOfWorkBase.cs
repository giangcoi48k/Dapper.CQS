using Microsoft.Extensions.Configuration;
using Polly.Registry;
using Polly.Retry;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.CQS
{
    public abstract class UnitOfWorkBase : IUnitOfWork
    {
        private readonly DbProviderFactory _dbProviderFactory;
        public DbConnection Connection { get; }
        public DbTransaction? Transaction { get; private set; }
        public IsolationLevel IsolationLevel { get; }

        private readonly CancellationToken _cancellationToken = default;
        private readonly RetryPolicy _retryPolicy;
        private readonly AsyncRetryPolicy _asyncRetryPolicy;
        private bool _disposed;

        public UnitOfWorkBase(ServiceFactory serviceFactory, UnitOfWorkOptions options)
        {
            OnConfiguring(serviceFactory.GetRequiredService<IConfiguration>(), options);
            var registry = serviceFactory.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            _retryPolicy = registry.Get<RetryPolicy>("sync");
            _asyncRetryPolicy = registry.Get<AsyncRetryPolicy>("async");
            _dbProviderFactory = options.DbProviderFactory;
            IsolationLevel = options.IsolationLevel;
            Connection = BuildConnection(options.ConnectionString);
        }

        protected virtual void OnConfiguring(IConfiguration configuration, UnitOfWorkOptions options)
        {

        }

        private DbConnection BuildConnection(string connectionString)
        {
            var connection = _dbProviderFactory.CreateConnection();
            if (connection == null)
                throw new Exception("Error initializing connection");
            connection.ConnectionString = connectionString;
            return connection;
        }

        public void Commit() => Transaction?.Commit();

        public Task CommitAsync() => CommitAsync(_cancellationToken);

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            if (Transaction != null)
            {
                await Transaction.CommitAsync(cancellationToken);
            }
        }

        public void Rollback() => Transaction?.Rollback();

        public Task RollbackAsync() => RollbackAsync(_cancellationToken);

        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            if (Transaction != null)
            {
                await Transaction.RollbackAsync(cancellationToken);
            }
        }

        public T? Query<T>(IQuery<T> query)
        {
            TransactionRequired(query);
            return _retryPolicy.Execute(() => query.Query(Connection, Transaction));
        }

        public Task<T?> QueryAsync<T>(IQueryAsync<T> query) => QueryAsync(query, _cancellationToken);

        public async Task<T?> QueryAsync<T>(IQueryAsync<T> query, CancellationToken cancellationToken)
        {
            await TransactionRequiredAsync(query, cancellationToken);
            return await _asyncRetryPolicy.ExecuteAsync(() => query.QueryAsync(Connection, Transaction, cancellationToken));
        }

        public void Execute(ICommand command)
        {
            TransactionRequired(command);
            _retryPolicy.Execute(() => command.Execute(Connection, Transaction));
        }

        public T? Execute<T>(ICommand<T> command)
        {
            TransactionRequired(command);
            return _retryPolicy.Execute(() => command.Execute(Connection, Transaction));
        }

        public Task ExecuteAsync(ICommandAsync command) => ExecuteAsync(command, _cancellationToken);

        public async Task ExecuteAsync(ICommandAsync command, CancellationToken cancellationToken)
        {
            await TransactionRequiredAsync(command, cancellationToken);
            await _asyncRetryPolicy.ExecuteAsync(() => command.ExecuteAsync(Connection, Transaction, cancellationToken));
        }

        public Task<T?> ExecuteAsync<T>(ICommandAsync<T> command) => ExecuteAsync(command, _cancellationToken);

        public async Task<T?> ExecuteAsync<T>(ICommandAsync<T> command, CancellationToken cancellationToken)
        {
            await TransactionRequiredAsync(command, cancellationToken);
            return await _asyncRetryPolicy.ExecuteAsync(() => command.ExecuteAsync(Connection, Transaction, cancellationToken));
        }

        protected virtual bool TransactionRequired(ITransactionRequired command)
        {
            if (command.TransactionRequired && Transaction == null)
            {
                if (Connection.State == ConnectionState.Closed)
                    Connection.Open();
                Transaction = Connection.BeginTransaction(IsolationLevel);
            }
            return true;
        }

        protected virtual async Task<bool> TransactionRequiredAsync(ITransactionRequired command, CancellationToken cancellationToken)
        {
            if (command.TransactionRequired && Transaction == null)
            {
                if (Connection.State == ConnectionState.Closed)
                    await Connection.OpenAsync(cancellationToken);
                Transaction = await Connection.BeginTransactionAsync(IsolationLevel, cancellationToken);
            }
            return true;
        }

        #region Finalizers
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~UnitOfWorkBase() => Dispose(false);

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
}
