using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Samples.PipelineBranching.Pipelines;

[assembly: FunctionsStartup(typeof(Samples.PipelineBranching.Startup))]

namespace Samples.PipelineBranching
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