using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Represents the middleware pipeline.
    /// </summary>
    public interface IMiddlewarePipeline
    {
        /// <summary>
        /// Gets or sets the exception handler.  Allows you to control how exceptions are handled and
        /// what status code and payload is returned in the response.
        /// </summary>
        [Obsolete("Use ExceptionHandlerMiddlware instead",  false)]
        Func<Exception, HttpContext, Task<IActionResult>> ExceptionHandler { get; set; }

        /// <summary>
        /// Adds middleware to the pipeline.
        /// </summary>
        /// <param name="middleware">The middleware to add.</param>
        /// <returns>The pipeline.</returns>
        IMiddlewarePipeline Use(IHttpMiddleware middleware);

        /// <summary>
        /// Executes the pipeline.
        /// </summary>
        /// <returns>The value to returned from the Azure function.</returns>
        Task<IActionResult> RunAsync();
    }
}
