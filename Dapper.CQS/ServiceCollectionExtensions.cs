using Dapper.CQS;
using Dapper.CQS.Internal;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using System;

namespace Dapper.CQS
{
    public static class ServiceCollectionExtensions
    {
        private static IServiceCollection ConfigureRetryPolicies<TContext>(this IServiceCollection services, int retryCount)
        {
            static TimeSpan Sleep(int retryAttempt) => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
            static bool ShouldRetryOn(IServiceProvider sp, Exception ex) => sp.GetRequiredService<IExceptionDetector>().ShouldRetryOn(ex);
            static void OnRetry(IServiceProvider sp, Exception ex, TimeSpan t) =>
                sp.GetRequiredService<ILogger<TContext>>().LogWarning(ex, "Retries after {TimeOut}s ({ExceptionMessage})", $"{t.TotalSeconds:n1}", ex.Message);

            services.TryAddSingleton<IExceptionDetector, SqlServerTransientExceptionDetector>();

            services.TryAddSingleton<IReadOnlyPolicyRegistry<string>>(sp =>
            {
                var registry = new PolicyRegistry()
                {
                    ["sync"] = Policy.Handle<SqlException>(ex => ShouldRetryOn(sp, ex)).WaitAndRetry(retryCount, Sleep, (ex, t) => OnRetry(sp, ex, t)),
                    ["async"] = Policy.Handle<SqlException>(ex => ShouldRetryOn(sp, ex)).WaitAndRetryAsync(retryCount, Sleep, (ex, t) => OnRetry(sp, ex, t))
                };
                return registry;
            });
            return services;
        }

        public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services, Action<UnitOfWorkOptions<TContext>>? options = null)
            where TContext : UnitOfWorkBase
        {
            var uowOptions = new UnitOfWorkOptions<TContext>();
            if (options != null)
            {
                options(uowOptions);
                uowOptions.IsConfigured = true;
            }
            services.AddSingleton(uowOptions);
            services.AddScoped<TContext>();
            services.TryAddTransient<ServiceFactory>(sp => sp.GetRequiredService);
            services.ConfigureRetryPolicies<TContext>(uowOptions.RetryCount);
            return services;
        }

        public static IServiceCollection AddUnitOfWork<TService, TContext>(this IServiceCollection services, Action<UnitOfWorkOptions<TContext>>? options = null)
            where TService : class, IUnitOfWork
            where TContext : UnitOfWorkBase, TService
        {
            var uowOptions = new UnitOfWorkOptions<TContext>();
            if (options != null)
            {
                options(uowOptions);
                uowOptions.IsConfigured = true;
            }
            services.AddSingleton(uowOptions);
            services.AddScoped<TService, TContext>();
            services.TryAddTransient<ServiceFactory>(sp => sp.GetRequiredService);
            services.ConfigureRetryPolicies<TContext>(uowOptions.RetryCount);
            return services;
        }
    }
}
