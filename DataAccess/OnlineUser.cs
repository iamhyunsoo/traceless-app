using System.Text;

namespace DataAccess;

// For Redis
public class OnlineUser
{
    public string UserId { get; set; } = default!;
    public string UserEmail { get; set; } = default!;
    public string Server { get; set; } = default!; // server user is connected to
    public bool IsOnline { get; set; }
}

public static class ServerInfo
{
	private static readonly string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
		"abcdefghijklmnopqrstuvwxyz" +
		"0123456789";

	public static string generateText(int length)
	{
		Random random = new Random();
		StringBuilder builder = new StringBuilder(length);

		for (int i = 0; i < length; ++i)
		{
			int index = random.Next(ALPHABET.Length);

			builder.Append(ALPHABET[index]);
		}

		return builder.ToString();
	}

	public static string Id = generateText(10);
	public const string AWS_SQS_QUEUE_NAME = "TracelessQueue";
}