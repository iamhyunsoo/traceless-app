using DataAccess;
using DataAccess.Data;
using DataAccess.Repository;
using Microsoft.EntityFrameworkCore;

namespace Server.Services;

public interface IChatService
{
    Task SendChatMessageToGroupMembers(string groupId, string sender, string message, bool delete);
    Task NotifyNewChatroom(string groupId, string receiver);
}

public class ChatService : IChatService
{
    private readonly ApplicationDbContext _db;
    private readonly IChatRepo _chatRepo;
    private readonly RedisPubSub _pubSub;
    private readonly IChatMessageService _chatMsgService;
    private readonly IConfiguration _configuration;

    public ChatService(ApplicationDbContext db, IChatRepo chatRepo, IChatMessageService chatMsgService, IConfiguration configuration)
    {
        _db = db;
        _chatRepo = chatRepo;
        _configuration = configuration;
        _chatMsgService = chatMsgService;
        _pubSub = new RedisPubSub(_configuration.GetConnectionString("MyRedisConStr")!, "PLACEHOLDER");
    }

    /// <summary>
    /// This is our main SEND function.
    /// This approach works when you have multiple web servers 
    /// because each web server can interact with the same data repository.
    /// </summary>
    public async Task SendChatMessageToGroupMembers(string groupId, string sender, string message, bool delete)
    {
        var chatRoom = await _chatRepo.GetChatRoom(groupId);
        if (chatRoom == null)
            return;
        else
        {
            // Add an item to AWS DynamoDB chat table
            int unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var msg = new ChatMessage
            {
                Id = groupId,
                CreatedAt = unixTimestamp,
                Sender = sender,
                Message = message
            };
            await _chatMsgService.AddChatMessageAsync(msg);

            // Check if each user of the chatRoom is online 
            var users = chatRoom.Users.ToList();
            for (int i = 0; i < users.Count; ++i)
            {
                OnlineUser? ui = await _chatRepo.GetUserInfoFromRedis(users[i].Id);
                if (ui == null)
                {
                    // Notify the receiver via email service or whatever..
                    continue;
                }
                if (delete)
                    _pubSub.Publish(users[i].Id, $"{PUB_SUB_TYPE.DELETE_CHAT}::{groupId}");
                else
                    _pubSub.Publish(users[i].Id, $"{PUB_SUB_TYPE.NEW_CHAT}::{groupId}");
            }
        }
    }

    public async Task NotifyNewChatroom(string groupId, string receiver)
    {
        var chatRoom = await _chatRepo.GetChatRoom(groupId);
        if (chatRoom == null)
            return;
        else
        {
            var user = await _db.Users.Where(x => x.Email == receiver).FirstOrDefaultAsync();
            if (user == null)
                return;
            else
            {
                OnlineUser? ui = await _chatRepo.GetUserInfoFromRedis(user.Id);
                if (ui == null)
                {
                    // Notify the receiver that s/he has some new messages.
                    return;
                }
                // Send a new chat room notification
                _pubSub.Publish(user.Id, $"{PUB_SUB_TYPE.NEW_ROOM}::{groupId}");
            }
        }
    }
}
