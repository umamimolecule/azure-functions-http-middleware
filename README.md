<p align="center">
<img src="https://raw.githubusercontent.com/umamimolecule/azure-functions-http-middleware/master/assets/obsolete.svg">
</p>

# azure-functions-http-middleware

![Azure DevOps builds (branch)](https://img.shields.io/azure-devops/build/umamimolecule/azure-functions-http-middleware/14/master) ![Azure DevOps coverage (branch)](https://img.shields.io/azure-devops/coverage/umamimolecule/azure-functions-http-middleware/14/master) ![Nuget](https://img.shields.io/nuget/v/Umamimolecule.AzureFunctionsMiddleware) [![](https://img.shields.io/badge/license-MIT-blue.svg)](#license)

An extensible middleware implementation for HTTP-triggered Azure Functions in .Net Core.

<p align="center">
<img src="https://raw.githubusercontent.com/umamimolecule/azure-functions-http-middleware/master/assets/logo.png">
</p>

Save yourself having to write the same cross-cutting concerns over and over for model validation, error handling, correlation IDs and such. This project was inspired by [this blog post](https://dasith.me/2018/01/20/using-azure-functions-httptrigger-as-web-api/) by Dasith Wijesiriwardena.

### Table of contents
 - [NuGet package](#nugetpackage)  
 - [Introduction](#introduction)  
 - [Dependencies](#dependencies)  
 - [Getting started](#gettingstarted)  
 - [Samples](#samples)  
 - [Built-in middleware](#builtinmiddleware)  
 - [Creating your own middleware](#creatingyourownmiddleware)  
 - [Pipeline branching](#pipelinebranching)  
 - [Conditional middleware](#conditionalmiddleware)  

---

<a name="nugetpackage" />

## NuGet package

https://www.nuget.org/packages/Umamimolecule.AzureFunctionsMiddleware/

<a name="introduction" />

## Introduction

At the moment, there isn't a built-in mechanism of defining middleware for Net Azure Functions written in .Net Core.  So this project aims to provide a similar middleware functionality that you would find in ASP .Net Core.

_**Update: Azure Functions written in .Net 5 and above using isolated process have [native support for middleware](https://docs.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide#middleware).  So if your project uses .Net 5 and above you probably want to use the native implementation instead of this project.**_

From Microsoft's documentation:

> Middleware is software that's assembled into an app pipeline to handle requests and responses. Each component:
> - Chooses whether to pass the request to the next component in the pipeline.
> - Can perform work before and after the next component in the pipeline.

As an example, we could add middleware to a HTTP-triggered Azure Function to extract a correlation ID from the request headers, validate the query parameters and then validate body parameters for a request like this:

**Startup.cs**
```
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(MyFunctionApp.Startup))]

namespace MyFunctionApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();
        }
    }
}
```

**MyFunctionApp.cs**
```
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Umamimolecule.AzureFunctionsMiddleware;

namespace MyFunctionApp
{
    public class MyFunction
    {
        private readonly IMiddlewarePipeline pipeline;

        public MyFunction(IHttpContextAccessor httpContextAccessor)
        {
            // This pipeline will:
            // 1. Extract correlation ID from request header 'request-id' and put into HttpContext.TraceIdentifier,
            // 2. Validate that required query parameters are present and put into HttpContext.Items["Query"]
            // 3. Validate the body payload contains all mandatory fields and put into HttpContext.Items["Body"]
            // 4. Executes the logic for this Azure Function
            //
            // Any validation errors will result in a 400 Bad Request returned.
            
            this.pipeline = new MiddlewarePipeline(httpContextAccessor);
            this.pipeline.UseCorrelationId(new string[] { "request-id" } )
                         .UseQueryValidation<QueryParameters>()
                         .UseBodyValidation<BodyPayload>()
                         .Use(this.ExecuteAsync);
        }

        [FunctionName(nameof(MyFunction))]
        public Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            return this.pipeline.RunAsync();
        }

        private Task<IActionResult> ExecuteAsync(HttpContext context)
        {
            // At this point, the query and body payloads have been validated, and
            // the correlation ID has been extracted from request headers.
            
            var payload = new
            {
                correlationId = context.TraceIdentifier,
                body = context.Items[ContextItems.Body],
                query = context.Items[ContextItems.Query]
            };

            return Task.FromResult<IActionResult>(new OkObjectResult(payload));
        }
    }

    public class QueryParameters
    {
        [Required]
        public string Id { get; set; }
    }

    public class BodyPayload
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
```

Calling the Azure Function without an `Id` query parameter would return the following error like this:

```
{
    "correlationId": "c223f312-6800-47cc-a905-675353f46c4d",
    "error": {
        "code": "INVALID_QUERY_PARAMETERS",
        "message": "The Id field is required."
    }
}
```

<a name="dependencies" />

## Dependencies
- Azure Functions SDK 3.0.11
- .Net Core 3.1

<a name="gettingstarted" />

## Getting Started

1. Run the following command in NuGet Package Manager Console (targetting your Azure Function project):
```
install-package Umamimolecule.AzureFunctionsMiddleware
```

2. Set up your `Startup.cs` class

Register the HTTP context accessor service - this is needed by the pipeline to determine the current HTTP context.

**Startup.cs**
```
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Samples.ConditionalMiddleware.Pipelines;

[assembly: FunctionsStartup(typeof(Example.Startup))]

namespace Example
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();
        }
    }
}
```

3. Set up your pipeline in your Azure Function

_Note: Middleware pipelines are configured within each Azure Function's constructor.  This is unlike ASP.Net Core where the pipelines are defined within the Startup class, and this is due to the way the Azure functions runtime works where it does not expose any_ `IApplicationBuilder` _type of bootstrapping._

**MyFunction.cs**
<pre><code>using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Umamimolecule.AzureFunctionsMiddleware;

namespace Example
{
    public class MyFunction
    {
        private readonly IMiddlewarePipeline pipeline;

<b>        public MyFunction(IHttpContextAccessor httpContextAccessor)
        {
            this.pipeline = new MiddlewarePipeline(httpContextAccessor);
            this.pipeline.UseCorrelationId(new string[] { "x-request-id" })
                         .Use(this.ExecuteAsync);            
        }
</b>
        [FunctionName(nameof(MyFunction))]
        public Task&lt;IActionResult&gt; Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            return this.pipeline.RunAsync();
        }

        private async Task&lt;IActionResult&gt; ExecuteAsync(HttpContext context)
        {
            var correlationId = context.TraceIdentifier;
        
            // Your function logic goes here...
        }
    }
}</code></pre>

4. In your HTTP trigger function, execute your pipeline:

**MyFunction.cs**
<pre><code>using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Umamimolecule.AzureFunctionsMiddleware;

namespace Example
{
    public class MyFunction
    {
        private readonly IMiddlewarePipeline pipeline;

        public MyFunction(IHttpContextAccessor httpContextAccessor)
        {
            this.pipeline = new MiddlewarePipeline(httpContextAccessor);
            this.pipeline.UseCorrelationId(new string[] { "x-request-id" })
                         .Use(this.ExecuteAsync);            
        }
<b>
        [FunctionName(nameof(MyFunction))]
        public Task&lt;IActionResult&gt; Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            return this.pipeline.RunAsync();
        }
</b>
        private async Task&lt;IActionResult&gt; ExecuteAsync(HttpContext context)
        {
            var correlationId = context.TraceIdentifier;
        
            // Your function logic goes here...
        }
    }
}</code></pre>

5. Then implement the logic for your Azure Function:

**MyFunction.cs**
<pre><code>using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Umamimolecule.AzureFunctionsMiddleware;

namespace Example
{
    public class MyFunction
    {
        private readonly IMiddlewarePipeline pipeline;

        public MyFunction(IHttpContextAccessor httpContextAccessor)
        {
            this.pipeline = new MiddlewarePipeline(httpContextAccessor);
            this.pipeline.UseCorrelationId(new string[] { "x-request-id" })
                         .Use(this.ExecuteAsync);            
        }

        [FunctionName(nameof(MyFunction))]
        public Task&lt;IActionResult&gt; Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            return this.pipeline.RunAsync();
        }
<b>
        private async Task&lt;IActionResult&gt; ExecuteAsync(HttpContext context)
        {
            var correlationId = context.TraceIdentifier;
        
            // Your function logic goes here...
        }
</b>    }
}</code></pre>

<a name="samples" />

## Samples

See the [Samples](https://github.com/umamimolecule/azure-functions-http-middleware/tree/master/samples) folder for some example use-cases.

<a name="builtinmiddleware" />

## Built-in middleware
This package comes with the following built-in middleware:

**BodyModelValidationMiddleware**  
Validates the body model for the request.  If successful, the body will be available in `HttpContext.Items["Body"]`.  Allows for a custom response to be returned if validation is unsuccessful.

**QueryModelValidationMiddleware**  
Validates the query model for the request.  If successful, the query object will be available in `HttpContext.Items["Query"]`.  Allows for a custom response to be returned if validation is unsuccessful.

**CorrelationIdMiddleware**  
Extracts a correlation ID from the request headers and sets the value to `HttpContext.TraceIdentifier`.  You can specify a collection of correlation ID header names and the first matching header will be used.  If no matching headers are found, a unique GUID will be used.

**ExceptionHandlerMiddleware**  
Allows exceptions to be handled and a custom response to be returned.

**FunctionMiddleware**  
Intended for your Azure Function implementation.

**RequestDelegateMiddleware**  
A general-purpose middleware for `RequestDelegate` instances.

<a name="creatingyourownmiddleware" />

## Creating your own middleware
You can implement `IHttpMiddleware` or sub-class the `HttpMiddleware` abstract class. Here's an example of some middleware to add a response header `x-request-date-utc` which contains the current UTC date and time of the request:
```
public class UtcRequestDateMiddleWare : HttpMiddleware
{
    public override Task InvokeAsync(HttpContext context)
    {
       context.Response.Headers["x-request-date-utc"] = System.DateTime.UtcNow.ToString("o");
    }
}
```

<a name="pipelinebranching" />

## Pipeline branching
You can add branching of a pipeline by using the `MapWhen` extension method:

```
// If Function1 is called, use MiddlewareA otherwise use MiddlewareB
pipeline.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/api/Function1"),
                 p => p.Use(middlewareA)
                       .Use(func))
        .Use(middlewareB)
        .Use(func);
```
 
This splits the middleware pipeline into two completely separate branches by specifying a predicate.  In this example, either middlewareA or middlewareB will be applied, but not both.

 - The first parameter for `MapWhen` is a predicate which returns true or false to indicate whether the branch should be run.
 - The second parameter is a function which take in the new pipeline branch, where you can add the middleware that should be run when the predicate returns true.

<a name="conditionalmiddleware" />

## Conditional middleware
You can add conditional middleware by using the `UseWhen` extension method:

```
// If Function1 is called, use MiddlewareA
pipeline.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api/Function1"),
                 p => p.Use(middlewareA));
        .Use(middlewareB)
        .Use(func);
```

This is similar to `MapWhen` but the difference is the main pipeline is rejoined after the branch, so in this example both middlewareA and middlewareB are run for Function1.
