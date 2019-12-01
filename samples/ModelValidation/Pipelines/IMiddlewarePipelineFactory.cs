using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umamimolecule.AzureFunctionsMiddleware;

namespace Samples.ModelValidation.Pipelines
{
    /// <summary>
    /// Represents a component to creates middleware pipeline instances used by this project.
    /// </summary>
    public interface IMiddlewarePipelineFactory
    {
        /// <summary>
        /// Creates a pipeline to validate query parameters.
        /// </summary>
        /// <typeparam name="TQuery">The object type representing the query parameters.</typeparam>
        /// <param name="func">The method containing the Azure Function business logic implementation.</param>
        /// <returns>The middleware pipeline.</returns>
        IMiddlewarePipeline CreateForQuery<TQuery>(Func<HttpContext, Task<IActionResult>> func) where TQuery : new();

        /// <summary>
        /// Creates a pipeline to validate a body payload.
        /// </summary>
        /// <typeparam name="TBody">The object type representing the body.</typeparam>
        /// <param name="func">The method containing the Azure Function business logic implementation.</param>
        /// <returns>The middleware pipeline.</returns>
        IMiddlewarePipeline CreateForBody<TBody>(Func<HttpContext, Task<IActionResult>> func) where TBody : new();

        /// <summary>
        /// Creates a pipeline to validate query parameters and a body payload.
        /// </summary>
        /// <typeparam name="TQuery">The object type representing the query parameters.</typeparam>
        /// <typeparam name="TBody">The object type representing the body.</typeparam>
        /// <param name="func">The method containing the Azure Function business logic implementation.</param>
        /// <returns>The middleware pipeline.</returns>
        IMiddlewarePipeline CreateForQueryAndBody<TQuery, TBody>(Func<HttpContext, Task<IActionResult>> func) where TQuery : new() where TBody : new();
    }
}
