using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umamimolecule.AzureFunctionsMiddleware;

namespace Samples.ConditionalBranching.Pipelines
{
    /// <summary>
    /// A component to creates middleware pipeline instances used by this project.
    /// </summary>
    public class MiddlewarePipelineFactory : IMiddlewarePipelineFactory
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly MiddlewareA middlewareA;
        private readonly MiddlewareB middlewareB;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewareFactory"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="middlewareA">Middleware A</param>
        /// <param name="middlewareB">Middleware B</param>
        public MiddlewarePipelineFactory(
            IHttpContextAccessor httpContextAccessor,
            MiddlewareA middlewareA,
            MiddlewareB middlewareB)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.middlewareA = middlewareA;
            this.middlewareB = middlewareB;
        }

        /// <summary>
        /// Creates a pipeline to validate query parameters.
        /// </summary>
        /// <typeparam name="TQuery">The object type representing the query parameters.</typeparam>
        /// <param name="func">The method containing the Azure Function business logic implementation.</param>
        /// <returns>The middleware pipeline.</returns>
        public IMiddlewarePipeline Create(Func<HttpContext, Task<IActionResult>> func)
        {
            MiddlewarePipeline pipeline = new MiddlewarePipeline(this.httpContextAccessor);

            // If Function1 is called, then use MiddlewareA which returns response header "x-middleware-a"
            // If Function2 is called, then use MiddlewareB which returns response header "x-middleware-b"
            return pipeline.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api/Function1"),
                                    p => p.Use(middlewareA))
                           .UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api/Function2"),
                                    p => p.Use(middlewareB))
                           .Use(func);
        }
    }
}
