using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShareOut.Internal;

namespace ShareOut.Microsoft.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddShare(this IServiceCollection services, Action<RedisOptions> action)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.Configure(action);
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<RedisOptions>>());
            services.AddSingleton<RedisPersistConnectionManager>();
            services.AddSingleton<IShare, Share>();
            return services;
        }
    }
}
