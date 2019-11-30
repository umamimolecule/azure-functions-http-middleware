using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Represents the middleware pipeline.
    /// </summary>
    public interface IMiddlewarePipeline
    {
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
