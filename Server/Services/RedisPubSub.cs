using StackExchange.Redis;

namespace Server.Services;

public class RedisPubSub
{
    private readonly IConnectionMultiplexer _redis;
    private readonly string _channelName;
    private readonly List<Action<string>> _handlers;
    public RedisPubSub(string connectionString, string channelName)
    {
        _redis = ConnectionMultiplexer.Connect(connectionString);
        _channelName = channelName;
        _handlers = new List<Action<string>>();
    }

    public void Subscribe(Action<string> handleMessage)
    {
        var subscriber = _redis.GetSubscriber();
        _handlers.Add(handleMessage);
        subscriber.Subscribe(_channelName, (channel, message) => handleMessage(message));
    }

    public void Unsubscribe(Action<string> handleMessage)
    {
        var subscriber = _redis.GetSubscriber();
        _handlers.Remove(handleMessage);
        if (_handlers.Count == 0)
        {
            subscriber.Unsubscribe(_channelName);
        }
    }

    public void Publish(string channelName, string message)
    {
        var publisher = _redis.GetSubscriber();
        publisher.Publish(channelName, message);
    }
}

public enum PUB_SUB_TYPE
{
    NEW_CHAT, 
    NEW_ROOM,
    DELETE_CHAT
}
