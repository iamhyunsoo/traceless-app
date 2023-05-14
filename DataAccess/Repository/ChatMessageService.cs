using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace DataAccess.Repository;

public interface IChatMessageService
{
    public Task<List<ChatMessage>> GetAllChatMessagesAsync();
    public Task<ChatMessage?> GetChatMessageAsync(string id, int createdAt);
    public Task<List<ChatMessage>> GetChatMessagesByPartitionKeyAsync(string id, long? since);
    public Task<List<ChatMessage>> GetPastChatMessagesByPartitionKeyAsync(string id, long until, int limit = 30);
    public Task<List<ChatMessage>> GetLastChatMessagesByPartitionKeyAsync(string id, int limit = 10);
    public Task AddChatMessageAsync(ChatMessage message);
    public Task UpdateChatMessageAsync(ChatMessage message);
    public Task DeleteChatMessageAsync(string id, int createdAt);
    public Task DeleteMessagesOlderThanAsync(int days);
}

public class ChatMessageService : IChatMessageService
{
    private readonly string _tableName = "chat";
    private readonly IAmazonDynamoDB _context;
    public ChatMessageService(IAmazonDynamoDB context)
    {
        _context = context;
    }

    public async Task<List<ChatMessage>> GetAllChatMessagesAsync()
    {
        var table = Table.LoadTable(_context, _tableName);
        var scanOptions = new ScanOperationConfig();
        var search = table.Scan(scanOptions);
        var messages = new List<ChatMessage>();

        do
        {
            var batch = await search.GetNextSetAsync();
            foreach (var document in batch)
            {
                var message = MapToChatMessage(document);
                messages.Add(message);
            }

        } while (!search.IsDone);

        return messages;
    }

    public async Task<ChatMessage?> GetChatMessageAsync(string id, int createdAt)
    {
        var table = Table.LoadTable(_context, _tableName);
        var key = new Document();
        key["id"] = id;
        key["createdAt"] = createdAt;
        var document = await table.GetItemAsync(key);
        if (document != null)
        {
            var message = MapToChatMessage(document);
            return message;
        }
        return null;
    }

    public async Task<List<ChatMessage>> GetChatMessagesByPartitionKeyAsync(string id, long? since)
    {
        var table = Table.LoadTable(_context, _tableName);
        var filter = new QueryFilter("id", QueryOperator.Equal, id);
        var queryOptions = new QueryOperationConfig { Filter = filter };
        if (since is not null)
            filter.AddCondition("createdAt", QueryOperator.GreaterThan, since);
        var search = table.Query(queryOptions);
        var messages = new List<ChatMessage>();

        do
        {
            var batch = await search.GetNextSetAsync();
            foreach (var document in batch)
            {
                var message = MapToChatMessage(document);
                messages.Add(message);
            }
        } while (!search.IsDone);

        return messages;
    }

    public async Task<List<ChatMessage>> GetPastChatMessagesByPartitionKeyAsync(string id, long until, int limit = 30)
    {
        var table = Table.LoadTable(_context, _tableName);
        var filter = new QueryFilter("id", QueryOperator.Equal, id);
        var queryOptions = new QueryOperationConfig 
        { 
            Filter = filter,
            Limit = limit,
            BackwardSearch = true
        };
        filter.AddCondition("createdAt", QueryOperator.LessThan, until);
        var search = table.Query(queryOptions);
        var messages = new List<ChatMessage>();
        var count = 0;
        do
        {
            var batch = await search.GetNextSetAsync();
            foreach (var document in batch)
            {
                var message = MapToChatMessage(document);
                messages.Add(message);
                count++;
                if (count == limit)
                {
                    messages.Reverse();
                    return messages;
                }
            }
        } while (!search.IsDone);
        messages.Reverse();
        return messages;
    }

    public async Task<List<ChatMessage>> GetLastChatMessagesByPartitionKeyAsync(string id, int limit = 10)
    {
        var table = Table.LoadTable(_context, _tableName);
        var filter = new QueryFilter("id", QueryOperator.Equal, id);
        var queryOptions = new QueryOperationConfig 
        { 
            Filter = filter, 
            Limit = limit,
            BackwardSearch = true
        };
        var search = table.Query(queryOptions);
        var messages = new List<ChatMessage>();
        var count = 0;
        do
        {
            var batch = await search.GetNextSetAsync();
            foreach (var document in batch)
            {
                var message = MapToChatMessage(document);
                messages.Add(message);
                count++;
                if (count == limit)
                {
                    messages.Reverse();
                    return messages;
                }
            }
        } while (!search.IsDone);
        messages.Reverse();
        return messages;
    }

    public async Task AddChatMessageAsync(ChatMessage message)
    {
        var table = Table.LoadTable(_context, _tableName);
        var document = MapToDocument(message);
        await table.PutItemAsync(document);
    }

    public async Task UpdateChatMessageAsync(ChatMessage message)
    {
        var table = Table.LoadTable(_context, _tableName);
        var document = MapToDocument(message);
        await table.UpdateItemAsync(document);
    }

    public async Task DeleteChatMessageAsync(string id, int createdAt)
    {
        var table = Table.LoadTable(_context, _tableName);
        var key = new Document();
        key["id"] = id;
        key["createdAt"] = createdAt;
        await table.DeleteItemAsync(key);
    }

    private ChatMessage MapToChatMessage(Document document)
    {
        var message = new ChatMessage();
        message.Id = document["id"].AsString();
        message.CreatedAt = document["createdAt"].AsInt();
        message.Sender = document.ContainsKey("sender") ? document["sender"].AsString() : null;
        message.Message = document.ContainsKey("message") ? document["message"].AsString() : null;
        return message;
    }

    private Document MapToDocument(ChatMessage message)
    {
        var document = new Document();
        document["id"] = message.Id;
        document["createdAt"] = message.CreatedAt;
        if (message.Sender != null) document["sender"] = message.Sender;
        if (message.Message != null) document["message"] = message.Message;
        return document;
    }

    public async Task DeleteMessagesOlderThanAsync(int days)
    {
        var table = Table.LoadTable(_context, _tableName);
        var currentDate = DateTimeOffset.UtcNow;
        var cutoffDate = currentDate.AddDays(-days).ToUnixTimeSeconds();

        var scanOptions = new ScanOperationConfig
        {
            Filter = new ScanFilter()
        };
        scanOptions.Filter.AddCondition("createdAt", ScanOperator.LessThan, cutoffDate);
        var search = table.Scan(scanOptions);

        do
        {
            var batch = await search.GetNextSetAsync();
            foreach (var document in batch)
            {
                var key = new Document();
                key["id"] = document["id"].AsString();
                key["createdAt"] = document["createdAt"].AsInt();
                await table.DeleteItemAsync(key);
            }
        } while (!search.IsDone);
    }
}