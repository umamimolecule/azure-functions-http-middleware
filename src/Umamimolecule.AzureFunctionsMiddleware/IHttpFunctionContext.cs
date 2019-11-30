//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;

//namespace Umamimolecule.AzureFunctionsMiddleware
//{
//    /// <summary>
//    /// Represents the context under which an Azure Function is executing.
//    /// </summary>
//    public interface IHttpFunctionContext2
//    {
//        /// <summary>
//        /// Gets the <see cref="HttpRequest"/> object which triggered the function.
//        /// </summary>
//        HttpRequest Request { get; }

//        /// <summary>
//        /// Gets or sets the response to be returned from the function.
//        /// </summary>
//        IActionResult Response { get; set; }

//        /// <summary>
//        /// Gets or sets the correlation id for the request.
//        /// </summary>
//        string CorrelationId { get; set; }

//        /// <summary>
//        /// Gets or sets the object containing the query parameters.
//        /// </summary>
//        object QueryModel { get; set; }

//        /// <summary>
//        /// Gets or sets the body payload.
//        /// </summary>
//        object BodyModel { get; set; }

//        /// <summary>
//        /// Gets or sets custom data.  Useful if you implement your own middleware
//        /// and want to store custom data in the context.
//        /// </summary>
//        IDictionary<string, object> Data { get; set; }
//    }
//}
