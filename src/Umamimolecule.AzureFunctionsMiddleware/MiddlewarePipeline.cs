using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// The middleware pipeline.
    /// </summary>
    public class MiddlewarePipeline : IMiddlewarePipeline
    {
        private readonly List<IHttpMiddleware> pipeline = new List<IHttpMiddleware>();
        private readonly IHttpContextAccessor httpContextAccessor;

        public MiddlewarePipeline(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Adds middleware to the pipeline.
        /// </summary>
        /// <param name="middleware">The middleware to add.</param>
        /// <returns>The pipeline.</returns>
        public IMiddlewarePipeline Use(IHttpMiddleware middleware)
        {
            if (pipeline.Any())
            {
                pipeline.Last().Next = middleware;
            }

            pipeline.Add(middleware);

            return this;
        }

        /// <summary>
        /// Executes the pipeline.
        /// </summary>
        /// <returns>The value to returned from the Azure function.</returns>
        public async Task<IActionResult> RunAsync()
        {
            var context = this.httpContextAccessor.HttpContext;

            if (pipeline.Any())
            {
                await pipeline.First().InvokeAsync(context);
            }

            return new HttpResponseResult(context);
        }
    }
}
