using Microsoft.Extensions.DependencyInjection;

namespace RCL.Core.Azure.BlobStorage
{
    public static class AzureBlobStorageServiceCollectionExtension
    {
        public static IServiceCollection AddRCLAzureBlobStorageServices(this IServiceCollection services,
            Action<AzureBlobStorageOptions> configureOptions)
        {
            services.AddTransient<IAzureBlobStorageService, AzureBlobStorageService>();
            services.Configure<AzureBlobStorageOptions>(configureOptions);
            return services;
        }
    }
}
