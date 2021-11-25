using Dapper.CQS.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Dapper.CQS
{
    public static class ServiceCollectionExtensions
    {
        private static readonly Type _repositoryType = typeof(IRepository);

        public static IServiceCollection AddUnitOfWork(this IServiceCollection services, Action<UnitOfWorkOptions> options, params Assembly[] assemblies)
        {
            var uowOptions = new UnitOfWorkOptions();
            options.Invoke(uowOptions);
            services.AddScoped<IUnitOfWorkFactory>(sp => new UnitOfWorkFactory(sp, uowOptions.ConnectionString, uowOptions.DbProvider));
            services.AddScoped(sp => sp.GetRequiredService<IUnitOfWorkFactory>().Create(
                  uowOptions.ConnectionString
                , uowOptions.Transactional
                , uowOptions.IsolationLevel
                , uowOptions.RetryOptions
            ));
            services.AddScoped(sp => sp.GetRequiredService<IUnitOfWorkFactory>().CreateAutoCommit(
                  uowOptions.ConnectionString
                , uowOptions.RetryOptions
            ));
            return services.AddRepositories(assemblies);
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies?.Any() != true) assemblies = new[] { Assembly.GetEntryAssembly() };
            foreach (var type in assemblies.Distinct().SelectMany(x => x.GetTypes().Where(FilterRepositories)))
            {
                services.AddScoped(GetServiceType(type), type);
            }
            return services;
        }

        private static Type GetServiceType(Type implType)
        {
            return implType
                .GetInterfaces()
                .Where(t => t != _repositoryType)
                .FirstOrDefault() ?? _repositoryType;
        }

        private static bool FilterRepositories(Type t)
        {
            return _repositoryType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract;
        }
    }
}
