using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Middleware to allow a request delegate to be executed.
    /// </summary>
    public class RequestDelegateMiddleware : HttpMiddleware
    {
        private readonly RequestDelegate requestDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestDelegateMiddleware"/> class.
        /// </summary>
        /// <param name="requestDelegate">The request delegate to be executed.</param>
        public RequestDelegateMiddleware(RequestDelegate requestDelegate)
        {
            this.requestDelegate = requestDelegate;
        }

        /// <summary>
        /// Runs the middleware.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override Task InvokeAsync(HttpContext context)
        {
            return this.requestDelegate(context);
        }
    }
}
