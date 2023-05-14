using DataAccess;

namespace Server.Services;

public static class Helpers
{
    /// <summary>
    /// Check the message's destination server.
    /// </summary>
    /// <param name="server"></param>
    /// <returns></returns>
    public static bool CheckDestination(string server)
    {
        return server.Equals(ServerInfo.Id);
    }

    public const string REDIS_CHAT = "::chat::";
    public const string CHAT_PREFIX_DB = "CHAT-";
    public const string S3_MEDIA_FILE_PREFIX = "ZS3gvj2m";
}

public class Pair<T1, T2>
{
    public T1 First { get; set; }
    public T2 Second { get; set; }

    public Pair(T1 first, T2 second)
    {
        First = first;
        Second = second;
    }
}