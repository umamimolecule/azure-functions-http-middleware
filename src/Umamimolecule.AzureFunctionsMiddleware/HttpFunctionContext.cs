using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// The context under which an Azure Function is executing.
    /// </summary>
    public class HttpFunctionContext : IHttpFunctionContext
    {
        /// <summary>
        /// Gets the <see cref="HttpRequest"/> object which triggered the function.
        /// </summary>
        public HttpRequest Request { get; set; }

        /// <summary>
        /// Gets or sets the response to be returned from the function.
        /// </summary>
        public IActionResult Response { get; set; }

        /// <summary>
        /// Gets or sets the correlation id for the request.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the object containing the query parameters.
        /// </summary>
        public object QueryModel { get; set; }

        /// <summary>
        /// Gets or sets the body payload.
        /// </summary>
        public object BodyModel { get; set; }

        /// <summary>
        /// Gets or sets custom data.  Useful if you implement your own middleware
        /// and want to store custom data in the context.
        /// </summary>
        public IDictionary<string, object> Data { get; set; }
    }
}
