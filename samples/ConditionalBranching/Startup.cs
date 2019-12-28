using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Samples.ConditionalBranching.Pipelines;

[assembly: FunctionsStartup(typeof(Samples.ConditionalBranching.Startup))]

namespace Samples.ConditionalBranching
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<IMiddlewarePipelineFactory, MiddlewarePipelineFactory>();
            builder.Services.AddTransient<MiddlewareA>();
            builder.Services.AddTransient<MiddlewareB>();
        }
    }
}