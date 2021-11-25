using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.CQS.Internal
{
    internal class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly DbProviderFactory _dbProviderFactory;
        private readonly string _connectionString;
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed;
        private HashSet<IAutoCommitUnitOfWork>? _unitOfWorks = new();

        public UnitOfWorkFactory(IServiceProvider serviceProvider, string connectionString, DbProviderFactory? dbProviderFactory = null)
        {
            _connectionString = connectionString;
            _serviceProvider = serviceProvider;
            _dbProviderFactory = dbProviderFactory ?? SqlClientFactory.Instance;
        }

        public IUnitOfWork Create(bool transactional = false, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, RetryOptions? retryOptions = null)
        {
            return Create(null, transactional, isolationLevel, retryOptions);
        }

        public IUnitOfWork Create(string? connectionString, bool transactional = false, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, RetryOptions? retryOptions = null)
        {
            var conn = BuildConnection(connectionString);
            conn.Open();
            return Return(new UnitOfWork(_serviceProvider, conn, transactional, isolationLevel, retryOptions));
        }

        public async Task<IUnitOfWork> CreateAsync(bool transactional = false, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, RetryOptions? retryOptions = null, CancellationToken cancellationToken = default)
        {
            return await CreateAsync(null, transactional, isolationLevel, retryOptions, cancellationToken);
        }

        public async Task<IUnitOfWork> CreateAsync(string? connectionString, bool transactional = false, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, RetryOptions? retryOptions = null, CancellationToken cancellationToken = default)
        {
            var conn = BuildConnection(connectionString);
            await conn.OpenAsync(cancellationToken);
            return Return(new UnitOfWork(_serviceProvider, conn, transactional, isolationLevel, retryOptions));
        }

        public IAutoCommitUnitOfWork CreateAutoCommit(RetryOptions? retryOptions = null)
        {
            return CreateAutoCommit(null, retryOptions);
        }

        public IAutoCommitUnitOfWork CreateAutoCommit(string? connectionString, RetryOptions? retryOptions = null)
        {
            var conn = BuildConnection(connectionString);
            conn.Open();
            return Return(new AutoCommitUnitOfWork(_serviceProvider, conn, retryOptions));
        }

        public async Task<IAutoCommitUnitOfWork> CreateAutoCommitAsync(RetryOptions? retryOptions = null, CancellationToken cancellationToken = default)
        {
            return await CreateAutoCommitAsync(null, retryOptions, cancellationToken);
        }

        public async Task<IAutoCommitUnitOfWork> CreateAutoCommitAsync(string? connectionString, RetryOptions? retryOptions = null, CancellationToken cancellationToken = default)
        {
            var conn = BuildConnection(connectionString);
            await conn.OpenAsync(cancellationToken);
            return Return(new AutoCommitUnitOfWork(_serviceProvider, conn, retryOptions));
        }

        private T Return<T>(T returnValue) where T : IAutoCommitUnitOfWork
        {
            _unitOfWorks?.Add(returnValue);
            return returnValue;
        }

        private DbConnection BuildConnection(string? connectionString = null)
        {
            var connection = _dbProviderFactory.CreateConnection();
            if (connection == null)
                throw new Exception("Error initializing connection");
            connection.ConnectionString = connectionString ?? _connectionString;
            return connection;
        }

        #region Finalizers

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~UnitOfWorkFactory() => Dispose(false);

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_unitOfWorks != null)
                {
                    foreach (var uow in _unitOfWorks)
                        uow?.Dispose();
                    _unitOfWorks.Clear();
                    _unitOfWorks = null;
                }
            }
            _disposed = true;
        }

        #endregion Finalizers
    }
}
