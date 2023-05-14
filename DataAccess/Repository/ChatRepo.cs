using System.Text;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using DataAccess.Data;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DataAccess.Repository;

public interface IChatRepo
{
    Task<ChatRoom?> GetChatRoom(string? id);
    Task<ChatRoom?> CreateChatRoom(string id, string name, List<string> users);
    Task<bool> DeleteChatRoom(string id);
    Task<ApplicationUser?> GetUser(string? id);
    Task<OnlineUser?> GetUserInfoFromRedis(string userid);
    Task<bool> SetUserInfoInRedis(string? userid, string server);
    Task<IEnumerable<ChatMessage>> GetItemsAsync(string partitionKey, long? since = null);
    Task<IEnumerable<ChatMessage>> GetPastItemsAsync(string partitionKey, long until, int limit = 30);
    Task<IEnumerable<ChatMessage>> GetLastItemsAsync(string partitionKey, int limit = 10);
    Task<string> UploadToS3(IBrowserFile file);
    string GeneratePreSignedUrl(string key);
}

public class ChatRepo : IChatRepo
{
    private readonly ApplicationDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly IChatMessageService _chatMsgService;
    private readonly IAmazonS3 _s3Client;
    private readonly long _MAXALLOWEDSIZE = 10 * 1024 * 1024;

    public ChatRepo(ApplicationDbContext db, IDistributedCache cache, IChatMessageService chatMsgService, IAmazonS3 s3Client)
    {
        _db = db;
        _cache = cache;
        _chatMsgService = chatMsgService;
        _s3Client = s3Client;
    }

    public async Task<ChatRoom?> GetChatRoom(string? id)
    {
        if (id is null)
            return null;
        var cr = await _db.ChatRooms.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == id);
        return cr;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name">group name</param>
    /// <param name="users">email list</param>
    /// <returns></returns>
    public async Task<ChatRoom?> CreateChatRoom(string id, string name, List<string> users)
    {
        var cr = new ChatRoom { Id = id, Name = string.Join(", ", users) };
        for (int i = 0; i < users.Count; ++i)
        {
            var user = await _db.Users.Where(x => x.Email == users[i]).FirstOrDefaultAsync();
            if (user is null)
                return null;
            cr.Users.Add(user);
            user.ChatRooms.Add(cr);
        }
        var res = await _db.ChatRooms.AddAsync(cr);
        await _db.SaveChangesAsync();
        return res.Entity;
    }

    public async Task<bool> DeleteChatRoom(string id)
    {
        var cr = await _db.ChatRooms.FindAsync(id);
        if (cr is null)
            return false;
        _db.ChatRooms.Remove(cr);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<ApplicationUser?> GetUser(string? id)
    {
        if (id is null)
            return null;
        var user = await _db.Users.Include(x => x.ChatRooms).FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
            return null;
        return user;
    }

    public async Task<OnlineUser?> GetUserInfoFromRedis(string userid)
    {
        byte[] cachedData = await _cache.GetAsync(userid);
        OnlineUser? user = null;
        if (cachedData is not null)
        {
            var cachedStr = Encoding.UTF8.GetString(cachedData);
            user = JsonSerializer.Deserialize<OnlineUser>(cachedStr);
        }
        return user;
    }

    public async Task<bool> SetUserInfoInRedis(string? userid, string server)
    {
        var user = await this.GetUser(userid);
        if (user is null)
            return false;

        var ou = new OnlineUser
        {
            UserId = user.Id,
            UserEmail = user.Email,
            Server = server,
            IsOnline = true
        };

        // Serialize the data to bytes
        var cachedStr = JsonSerializer.Serialize(ou);
        var cachedBytes = Encoding.UTF8.GetBytes(cachedStr);

        await _cache.SetAsync(userid, cachedBytes);
        return true;
    }

    public async Task<IEnumerable<ChatMessage>> GetItemsAsync(string partitionKey, long? since = null)
    {
        var msgs = await _chatMsgService.GetChatMessagesByPartitionKeyAsync(partitionKey, since);
        return msgs;
    }

    public async Task<IEnumerable<ChatMessage>> GetPastItemsAsync(string partitionKey, long until, int limit = 30)
    {
        var msgs = await _chatMsgService.GetPastChatMessagesByPartitionKeyAsync(partitionKey, until, limit);
        return msgs;
    }

    public async Task<IEnumerable<ChatMessage>> GetLastItemsAsync(string partitionKey, int limit = 10)
    {
        var msgs = await _chatMsgService.GetLastChatMessagesByPartitionKeyAsync(partitionKey, limit);
        return msgs;
    }

    public async Task<string> UploadToS3(IBrowserFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.OpenReadStream(_MAXALLOWEDSIZE).CopyToAsync(memoryStream);

        // Generate a unique name using a GUID and the original file extension
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}";

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = memoryStream,
            Key = uniqueFileName,
            BucketName = "traceless",
            ContentType = file.ContentType,
            //CannedACL = S3CannedACL.PublicRead // Set the access control as needed
        };

        var transferUtility = new TransferUtility(_s3Client);
        await transferUtility.UploadAsync(uploadRequest);

        return uniqueFileName;
    }

    public string GeneratePreSignedUrl(string key)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = "traceless",
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(10) // Set the expiration time as needed
        };

        return _s3Client.GetPreSignedURL(request);
    }
}

