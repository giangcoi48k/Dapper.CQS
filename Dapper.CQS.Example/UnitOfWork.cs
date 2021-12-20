namespace Dapper.CQS.Example
{
    public class UnitOfWork : UnitOfWorkBase
    {
        public UnitOfWork(ServiceFactory serviceFactory, UnitOfWorkOptions<UnitOfWork> options)
            : base(serviceFactory, options)
        {
        }

        protected override void OnConfiguring(IConfiguration configuration, UnitOfWorkOptions options)
        {
            if (options.IsConfigured) return;
            options.ConnectionString = "Data Source=localhost;database=RealEstate;Integrated Security=true;TrustServerCertificate=True";
            options.RetryCount = 3;
        }
    }
}
