using System;

namespace Dapper.CQS
{
    public interface IExceptionDetector
    {
        bool ShouldRetryOn(Exception ex);
    }
}