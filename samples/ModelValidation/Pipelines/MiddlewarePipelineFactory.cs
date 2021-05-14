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
        /// Creates a pipeline to validate query parameters.
        /// </summary>
        /// <typeparam name="TQuery">The object type representing the query parameters.</typeparam>
        /// <param name="func">The method containing the Azure Function business logic implementation.</param>
        /// <param name="handleValidationFailure">Optional handler to return a custom response when validation is unsuccessful.</param>
        /// <returns>The middleware pipeline.</returns>
        public IMiddlewarePipeline CreateForQuery<TQuery>(
            Func<HttpContext, Task<IActionResult>> func,
            Func<HttpContext, ModelValidationResult, IActionResult> handleValidationFailure = null)
            where TQuery : new()
        {
            MiddlewarePipeline pipeline = new MiddlewarePipeline(this.httpContextAccessor);
            return pipeline.UseCorrelationId(correlationIdHeaders)
                           .UseQueryValidation<TQuery>(handleValidationFailure)
                           .Use(func);
        }

        /// <summary>
        /// Creates a pipeline to validate a body payload.
        /// </summary>
        /// <typeparam name="TBody">The object type representing the body.</typeparam>
        /// <param name="func">The method containing the Azure Function business logic implementation.</param>
        /// <param name="handleValidationFailure">Optional handler to return a custom response when validation is unsuccessful.</param>
        /// <returns>The middleware pipeline.</returns>
        public IMiddlewarePipeline CreateForBody<TBody>(
            Func<HttpContext, Task<IActionResult>> func,
            Func<HttpContext, ModelValidationResult, IActionResult> handleValidationFailure = null)
            where TBody : new()
        {
            MiddlewarePipeline pipeline = new MiddlewarePipeline(this.httpContextAccessor);
            return pipeline.UseCorrelationId(correlationIdHeaders)
                           .UseBodyValidation<TBody>(handleValidationFailure)
                           .Use(func);
        }
    }
}
