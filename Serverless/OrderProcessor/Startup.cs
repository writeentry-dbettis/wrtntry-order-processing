using System.ComponentModel.Design.Serialization;
using BatchProcessing.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OrderProcessor.Services;

namespace OrderProcessor;

public class Startup : Google.Cloud.Functions.Hosting.FunctionsStartup
{
    public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
    {
        services.Configure<OrderProcessorOptions>(
            context.Configuration.GetSection("OrderProcessor"));

        services.Configure<GcpCloudStorageProviderOptions>(
            context.Configuration.GetSection("StorageOptions"));
        
        services.AddSingleton<IStorageProvider, GcpCloudStorageProvider>();
    }
}
