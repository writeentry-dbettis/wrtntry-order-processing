using System.Text.Json;
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

        builder.Services.AddControllers()
            .AddJsonOptions(opts => {
                opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        // add service dependencies
        builder.Services.AddScoped<IBatchProcessor, CsvBatchProcessor>();
        builder.Services.AddScoped<IPublishService, QueuePublishService>();

        builder.Services.AddHostedService<QueuePublishService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseDefaultFiles();

        app.UseStaticFiles();

        app.UseCors(cors => {
            cors.AllowAnyMethod();
            cors.AllowAnyHeader();
            cors.AllowCredentials();
            
            if (app.Environment.IsDevelopment())
            {
                cors.WithOrigins("http://localhost:4200", 
                    "https://batch-processing-web-555794641073.us-east1.run.app");
            }
        });

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllers();

        app.MapHub<ChatHub>("/hub");

        app.Run();
    }
}
