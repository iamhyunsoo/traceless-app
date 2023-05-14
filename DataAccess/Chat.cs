using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class ChatRoom
{
    [Key]
    public string Id { get; set; } = default!; // Unique group id
    public string Name { get; set; } = string.Empty;    // Chat room's name
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}

[Index(nameof(Id), nameof(ChatRoomId))]
public class ChatHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string ChatRoomId { get; set; } = default!;
    public string Message { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Handle this dateTime properly using daylightsaving checker

    // NOT IMPLEMENTED
    // string S3ObjectUrl
}

[DynamoDBTable("chat")]
public class ChatMessage
{
    [DynamoDBHashKey("id")]
    public string Id { get; set; } = default!;

    [DynamoDBRangeKey("createdAt")]
    public long CreatedAt { get; set; }

    [DynamoDBProperty("sender")]
    public string? Sender { get; set; }

    [DynamoDBProperty("message")]
    public string? Message { get; set; }
}