using BatchProcessingApi.Hubs;
using BatchProcessingApi.Interfaces;
using BatchProcessingApi.Services;
using Google.Rpc;
using Microsoft.AspNetCore.SignalR;

namespace BatchProcessingApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<QueuePublishServiceConfiguration>(
            builder.Configuration.GetSection("PublishQueue"));

        builder.Services.AddSignalR();

        builder.Services.AddHostedService<PublisherService>();

        builder.Services.AddControllers();

        // add service dependencies
        builder.Services.AddScoped<IBatchProcessor, CsvBatchProcessor>();
        builder.Services.AddScoped<IPublishService, QueuePublishService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseDefaultFiles();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllers();

        app.MapHub<ChatHub>("/hub");

        app.Run();
    }
}

public class PublisherService : BackgroundService
{
    private static string[] _statuses = { "Queued", "Processing", "Completed" };
    private readonly IHubContext<ChatHub, IChatClient> _hub;

    public PublisherService(IHubContext<ChatHub, IChatClient> hub)
    {
        _hub = hub;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            var newId = DateTime.Now.Ticks;

            for (int status = 0; status < _statuses.Length; status++)
            {
                await _hub.Clients.Groups("11111111-0000-2222-4444-333333333333").StatusChanged(newId.ToString(), _statuses[status]);
                await Task.Delay(500);
            }
        }
    }
}