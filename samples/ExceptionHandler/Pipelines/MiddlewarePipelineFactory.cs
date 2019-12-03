using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umamimolecule.AzureFunctionsMiddleware;

namespace Samples.ModelValidation.Pipelines
{
    /// <summary>
    /// A component to creates middleware pipeline instances used by this project.
    /// </summary>
    public class MiddlewarePipelineFactory : IMiddlewarePipelineFactory
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// The request headers containing correlation identifiers.
        /// </summary>
        private readonly static string[] correlationIdHeaders = new string[]
        {
            "ms-request-id",
            "request-id"
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewareFactory"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public MiddlewarePipelineFactory(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Creates a pipeline to demonstrate exception handling.
        /// </summary>
        /// <param name="func">The method containing the Azure Function business logic implementation.</param>
        /// <returns>The middleware pipeline.</returns>
        public IMiddlewarePipeline Create(Func<HttpContext, Task<IActionResult>> func)
        {
            MiddlewarePipeline pipeline = new MiddlewarePipeline(this.httpContextAccessor);
            return pipeline.UseCorrelationId(correlationIdHeaders)
                           .UseExceptionHandling(CustomExceptionHandler.HandleAsync)
                           .Use(func);
        }
    }
}
