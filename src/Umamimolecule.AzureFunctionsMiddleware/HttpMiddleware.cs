using System.Threading.Tasks;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Base class for all middleware.
    /// </summary>
    public abstract class HttpMiddleware : IHttpMiddleware
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMiddleware"/> class.
        /// </summary>
        protected HttpMiddleware()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware to be run.</param>
        protected HttpMiddleware(IHttpMiddleware next)
        {
            this.Next = next;
        }

        /// <summary>
        /// Gets or sets the next middleware to be executed after this one.
        /// </summary>
        public IHttpMiddleware Next { get; set; }

        /// <summary>
        /// Runs the middleware.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public abstract Task InvokeAsync(IHttpFunctionContext context);
    }
}
