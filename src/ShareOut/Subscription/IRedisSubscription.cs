using System.Threading.Tasks;

namespace ShareOut.Subscription
{
    public interface IRedisSubscription
    {
        Task PublishAsync<T>(string key, T value) where T : class, new();
        Task SubscribeAsync(string key, IConsumer consumer);
    }
}
