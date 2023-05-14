

using Microsoft.AspNetCore.Identity;

namespace DataAccess;

public class ApplicationUser : IdentityUser
{
    public ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();
}
