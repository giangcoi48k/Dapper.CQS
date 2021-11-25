using System.Collections.Generic;

namespace Dapper.CQS
{
    public interface IPaged
    {
        int Page { get; set; }
        int PageSize { get; set; }
        long TotalRecords { get; set; }
        long TotalPage { get; }
        bool HasNextPage { get; }
        bool HasPreviousPage { get; }
    }

    public interface IPaged<T> : IPaged
    {
        IReadOnlyCollection<T> Items { get; set; }
    }
}
