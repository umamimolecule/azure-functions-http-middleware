using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Middleware to conditionally execute a branch.
    /// </summary>
    public class ConditionalMiddleware : HttpMiddleware
    {
        private readonly IMiddlewarePipeline pipeline;
        private readonly Action<IMiddlewarePipeline> configure;
        private readonly Func<HttpContext, bool> condition;
        private readonly bool rejoinPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalMiddleware"/> class.
        /// </summary>
        /// <param name="pipeline">The pipeline. Required.</param>
        /// <param name="condition">The condition to evaluate. Required.</param>
        /// <param name="configure">Configures the branch. Optional.</param>
        /// <param name="rejoinPipeline">Determines if the branch should rejoin the main pipeline or not.</param>
        public ConditionalMiddleware(
            IMiddlewarePipeline pipeline,
            Func<HttpContext, bool> condition,
            Action<IMiddlewarePipeline> configure,
            bool rejoinPipeline)
        {
            GuardClauses.IsNotNull(nameof(pipeline), pipeline);
            GuardClauses.IsNotNull(nameof(condition), condition);

            this.pipeline = pipeline;
            this.configure = configure;
            this.condition = condition;
            this.rejoinPipeline = rejoinPipeline;
        }

        /// <summary>
        /// Runs the middleware.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task InvokeAsync(HttpContext context)
        {
            if (this.condition(context))
            {
                // Create new pipeline for branch
                var branch = this.pipeline.New();
                this.configure?.Invoke(branch);
                await branch.RunAsync();

                if (!this.rejoinPipeline)
                {
                    return;
                }
            }

            if (this.Next != null)
            {
                await this.Next.InvokeAsync(context);
            }
        }
    }
}
