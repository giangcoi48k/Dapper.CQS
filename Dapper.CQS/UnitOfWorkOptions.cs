using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace Dapper.CQS
{
    public class UnitOfWorkOptions
    {
        public string ConnectionString { get; set; } = default!;
        public DbProviderFactory DbProviderFactory { get; set; } = SqlClientFactory.Instance;
        public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;
        public bool Transactional { get; set; } = true;
        public int RetryCount { get; set; } = 5;
        public bool IsConfigured { get; set; }
    }

    public class UnitOfWorkOptions<Tcontext> : UnitOfWorkOptions
        where Tcontext : UnitOfWorkBase
    {

    }
}
