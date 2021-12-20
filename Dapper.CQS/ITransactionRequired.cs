namespace Dapper.CQS
{
    public interface ITransactionRequired
    {
        bool TransactionRequired { get; }
    }
}
