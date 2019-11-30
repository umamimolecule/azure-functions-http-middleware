using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Represents a middleware component.
    /// </summary>
    public interface IHttpMiddleware
    {
        /// <summary>
        /// Gets or sets the next middleware to be executed after this one.
        /// </summary>
        IHttpMiddleware Next { get; set; }

        /// <summary>
        /// Runs the middleware.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InvokeAsync(HttpContext context);
    }
}
