using DataAccess.Data;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DataAccess;
using DataAccess.Repository;
using Server.Services;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.Extensions.NETCore.Setup;
using Amazon;
using Quartz;
using Amazon.Runtime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ConfigurationManager configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(
            dbContextOptions => dbContextOptions
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                // The following three options help with debugging, but should
                // be changed or removed for production.
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
        );

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("MyRedisConStr");
    options.InstanceName = "TracelessRedis";
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>
    (options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);


////////////////////////////////////////////////////////////////////////////////////////////////////
//// Use your aws credentials with lines below if you have to hard code it in appsettings.json /////
////////////////////////////////////////////////////////////////////////////////////////////////////
//var awsOptions = configuration.GetAWSOptions();
//builder.Services.AddDefaultAWSOptions(awsOptions);
//builder.Services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(RegionEndpoint.USEast2));
//builder.Services.AddSingleton<IDynamoDBContext>(sp => new DynamoDBContext(sp.GetRequiredService<IAmazonDynamoDB>()));
//builder.Services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(RegionEndpoint.USEast2));
//////////////////////////////////////// END ////////////////////////////////////////////////////////

// Comment out the below 3 lines if you hard code aws credentials with the above lines
builder.Services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(RegionEndpoint.USEast2));
builder.Services.AddSingleton<IDynamoDBContext>(sp => new DynamoDBContext(sp.GetRequiredService<IAmazonDynamoDB>()));
builder.Services.AddAWSService<IAmazonS3>(new AWSOptions {Region = RegionEndpoint.USEast2 });

builder.Services.AddScoped<IChatMessageService, ChatMessageService>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<UniqueIdService>();
builder.Services.AddScoped<IChatRepo, ChatRepo>();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

//Schedules
builder.Services.AddQuartz(q =>
{
    // handy when part of cluster or you want to otherwise identify multiple schedulers
    q.SchedulerId = "Scheduler-Core";

    // we take this from appsettings.json, just show it's possible
    // q.SchedulerName = "Quartz ASP.NET Core Sample Scheduler";

    // as of 3.3.2 this also injects scoped services (like EF DbContext) without problems
    q.UseMicrosoftDependencyInjectionJobFactory();
    // or for scoped service support like EF Core DbContext
    // q.UseMicrosoftDependencyInjectionScopedJobFactory();

    // these are the defaults
    q.UseSimpleTypeLoader();
    q.UseInMemoryStore();
    q.UseDefaultThreadPool(tp =>
    {
        tp.MaxConcurrency = 10;
    });
    // quickest way to create a job with single trigger is to use ScheduleJob
    // (requires version 3.2)
    q.ScheduleJob<DataAccess.Scheduler.DeleteOldMessagesJob>(trigger => trigger
        .WithIdentity("TracelessPrivacyPolicy")
        .StartNow()
        .WithSimpleSchedule(x => x
            .WithIntervalInHours(1)
            .RepeatForever())
        .WithDescription("DeleteOldMessages schedule")
    );
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();
app.UseResponseCompression();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.UseAuthentication();

using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;
    var dbInitializer = services.GetRequiredService<IDbInitializer>();
    dbInitializer.Initialize();
}

app.Run();
