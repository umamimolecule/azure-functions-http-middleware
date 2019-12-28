using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umamimolecule.AzureFunctionsMiddleware;

namespace Samples.PipelineBranching.Pipelines
{
    /// <summary>
    /// Represents a component to creates middleware pipeline instances used by this project.
    /// </summary>
    public interface IMiddlewarePipelineFactory
    {
        /// <summary>
        /// Creates a pipeline.
        /// </summary>
        /// <param name="func">The method containing the Azure Function business logic implementation.</param>
        /// <returns>The middleware pipeline.</returns>
        IMiddlewarePipeline Create(Func<HttpContext, Task<IActionResult>> func);
    }
}
