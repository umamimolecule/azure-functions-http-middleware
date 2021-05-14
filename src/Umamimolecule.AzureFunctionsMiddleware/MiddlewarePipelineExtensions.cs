﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Contains extension methods for <see cref="IMiddlewarePipeline"/> instances.
    /// </summary>
    public static class MiddlewarePipelineExtensions
    {
        /// <summary>
        /// Adds an Azure Function middleware to the pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="func">The function to add.</param>
        /// <returns>The pipeline instance.</returns>
        public static IMiddlewarePipeline Use(this IMiddlewarePipeline pipeline, Func<HttpContext, Task<IActionResult>> func)
        {
            return pipeline.Use(new FunctionMiddleware(func));
        }

        /// <summary>
        /// Adds a request delegate middleware to the pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="requestDelegate">The request delegate to add.</param>
        /// <returns>The pipeline instance.</returns>
        public static IMiddlewarePipeline Use(this IMiddlewarePipeline pipeline, RequestDelegate requestDelegate)
        {
            return pipeline.Use(new RequestDelegateMiddleware(requestDelegate));
        }

        /// <summary>
        /// Adds correlation ID middleware to the pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="correlationIdHeaders">The colleciton of request headers that contain the correlation ID.  The first match is used.</param>
        /// <returns>The pipeline instance.</returns>
        public static IMiddlewarePipeline UseCorrelationId(this IMiddlewarePipeline pipeline, IEnumerable<string> correlationIdHeaders)
        {
            return pipeline.Use(new CorrelationIdMiddleware(correlationIdHeaders));
        }

        /// <summary>
        /// Adds query parameter validation middleware to the pipeline.
        /// </summary>
        /// <typeparam name="T">The query model type.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="handleValidationFailure">Optional handler to return a custom response when validation is unsuccessful.</param>
        /// <returns>The pipeline instance.</returns>
        public static IMiddlewarePipeline UseQueryValidation<T>(
            this IMiddlewarePipeline pipeline,
            Func<HttpContext, ModelValidationResult, IActionResult> handleValidationFailure = null)
            where T : new()
        {
            return pipeline.Use(new QueryModelValidationMiddleware<T>() { HandleValidationFailure = handleValidationFailure });
        }

        /// <summary>
        /// Adds body payload validation middleware to the pipeline.
        /// </summary>
        /// <typeparam name="T">The body model type.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="handleValidationFailure">Optional handler to return a custom response when validation is unsuccessful.</param>
        /// <returns>The pipeline instance.</returns>
        public static IMiddlewarePipeline UseBodyValidation<T>(
            this IMiddlewarePipeline pipeline,
            Func<HttpContext, ModelValidationResult, IActionResult> handleValidationFailure = null)
            where T : new()
        {
            return pipeline.Use(new BodyModelValidationMiddleware<T>() { HandleValidationFailure = handleValidationFailure });
        }

        /// <summary>
        /// Adds exception handling middleware to the pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="exceptionHandler">An optional handler to process exceptions.  Leave null to use the default exception handler.</param>
        /// <param name="loggerHandler">An optional handler to log exceptions.</param>
        /// <returns>The pipeline instance.</returns>
        public static IMiddlewarePipeline UseExceptionHandling(
            this IMiddlewarePipeline pipeline,
            Func<Exception, HttpContext, Task<IActionResult>> exceptionHandler = null,
            Func<Exception, Task> loggerHandler = null)
        {
            var middleware = new ExceptionHandlerMiddleware()
            {
                ExceptionHandler = exceptionHandler ?? ExceptionHandlerMiddleware.DefaultExceptionHandler,
                LogExceptionAsync = loggerHandler,
            };

            return pipeline.Use(middleware);
        }

        /// <summary>
        /// Conditionally creates a branch in the request pipeline that is rejoined to the main pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="condition">The function which is invoked to determine if the branch should be taken.</param>
        /// <param name="configure">Configures the branch.</param>
        /// <returns>The pipeline instance.</returns>
        public static IMiddlewarePipeline UseWhen(
            this IMiddlewarePipeline pipeline,
            Func<HttpContext, bool> condition,
            Action<IMiddlewarePipeline> configure)
        {
            var middleware = new ConditionalMiddleware(pipeline, condition, configure, true);
            return pipeline.Use(middleware);
        }

        /// <summary>
        /// Conditionally creates a branch in the request pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="condition">The function which is invoked to determine if the branch should be taken.</param>
        /// <param name="configure">Configures the branch.</param>
        /// <returns>The pipeline instance.</returns>
        public static IMiddlewarePipeline MapWhen(
            this IMiddlewarePipeline pipeline,
            Func<HttpContext, bool> condition,
            Action<IMiddlewarePipeline> configure)
        {
            var middleware = new ConditionalMiddleware(pipeline, condition, configure, false);
            return pipeline.Use(middleware);
        }
    }
}
