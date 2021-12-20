using System;

namespace Dapper.CQS
{
    public delegate object ServiceFactory(Type serviceType);

    public static class ServiceFactoryExtension
    {
        public static TService GetRequiredService<TService>(this ServiceFactory factory)
        {
            return (TService)factory(typeof(TService));
        }
    }
}
