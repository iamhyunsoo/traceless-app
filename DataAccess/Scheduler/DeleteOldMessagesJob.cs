using DataAccess.Repository;
using Microsoft.Extensions.Configuration;
using Quartz;

namespace DataAccess.Scheduler;

public class DeleteOldMessagesJob : IJob
{
    private readonly IChatMessageService _chatMessageService;
    private readonly IConfiguration _configuration;

    public DeleteOldMessagesJob(IChatMessageService chatMessageService, IConfiguration configuration)
    {
        _chatMessageService = chatMessageService;
        _configuration = configuration;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var retentionDays = _configuration["MessageRetentionDays"];
        int days = 7;   // By default, it is 7 days.
        if (retentionDays is not null)
            days = Int32.Parse(retentionDays);
        await _chatMessageService.DeleteMessagesOlderThanAsync(days);
    }
}
