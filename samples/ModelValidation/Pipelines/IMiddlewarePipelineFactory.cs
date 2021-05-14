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
        /// <param name="handleValidationFailure">Optional handler to return a custom response when validation is unsuccessful.</param>
        /// <returns>The middleware pipeline.</returns>
        IMiddlewarePipeline CreateForQuery<TQuery>(
            Func<HttpContext, Task<IActionResult>> func,
            Func<HttpContext, ModelValidationResult, IActionResult> handleValidationFailure = null)
            where TQuery : new();

        /// <summary>
        /// Creates a pipeline to validate a body payload.
        /// </summary>
        /// <typeparam name="TBody">The object type representing the body.</typeparam>
        /// <param name="func">The method containing the Azure Function business logic implementation.</param>
        /// <param name="handleValidationFailure">Optional handler to return a custom response when validation is unsuccessful.</param>
        /// <returns>The middleware pipeline.</returns>
        IMiddlewarePipeline CreateForBody<TBody>(
            Func<HttpContext, Task<IActionResult>> func,
            Func<HttpContext, ModelValidationResult, IActionResult> handleValidationFailure = null)
            where TBody : new();
    }
}
