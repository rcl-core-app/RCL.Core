using Microsoft.Extensions.DependencyInjection;

namespace RCL.Core.Identity.Graph
{
    public static class GraphExtension
    {
        public static IServiceCollection AddRCLIdentityGraphServices(this IServiceCollection services, 
            Action<GraphOptions> setupAction)
        {
            services.AddTransient<IGraphService, GraphService>();
            services.Configure(setupAction);
            return services;
        }
    }
}
