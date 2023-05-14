namespace Server.Services
{
    public class UniqueIdService
    {
        public string UniqueId { get; }
        public UniqueIdService()
        {
            UniqueId = Guid.NewGuid().ToString();
        }
    }
}