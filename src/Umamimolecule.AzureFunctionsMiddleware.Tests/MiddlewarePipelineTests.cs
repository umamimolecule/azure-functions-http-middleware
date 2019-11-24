using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class MiddlewarePipelineTests
    {
        [Fact(DisplayName = "Use should add the first middleware successfully")]
        public void Use_FirstMiddleWare()
        {
            var instance = this.CreateInstance();
            var middleware = new Mock<IHttpMiddleware>();
            instance.Use(middleware.Object);
            middleware.Verify(x => x.Next, Times.Never);
        }

        [Fact(DisplayName = "Use should add the second middleware successfully")]
        public void Use_SecondMiddleWare()
        {
            var instance = this.CreateInstance();
            var middleware1 = new Mock<IHttpMiddleware>();
            var middleware2 = new Mock<IHttpMiddleware>();
            instance.Use(middleware1.Object);
            instance.Use(middleware2.Object);
            middleware1.VerifySet(x => x.Next = middleware2.Object, Times.Once);
            middleware2.VerifySet(x => x.Next = It.IsAny<IHttpMiddleware>(), Times.Never);
        }

        [Fact(DisplayName = "RunAsync should return expected response when no middleware is configured")]
        public async Task RunAsync_NoMiddleware_NoCustomExceptionHandler()
        {
            var instance = this.CreateInstance();
            var request = new Mock<HttpRequest>();
            var result = await instance.RunAsync(request.Object);
            var objectResult = result.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
        }

        [Fact(DisplayName = "RunAsync should return expected response when last middleware does not return a response")]
        public async Task RunAsync_LastMiddlewareNoResponse_NoCustomExceptionHandler()
        {
            var instance = this.CreateInstance();
            var middleware = new Mock<IHttpMiddleware>();
            instance.Use(middleware.Object);
            var request = new Mock<HttpRequest>();
            var result = await instance.RunAsync(request.Object);
            var objectResult = result.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
        }

        [Fact(DisplayName = "RunAsync should return expected response when middleware throws BadRequestException")]
        public async Task RunAsync_MiddlewareThrowsBadRequestException_NoCustomExceptionHandler()
        {
            var instance = this.CreateInstance();
            var middleware = new Mock<IHttpMiddleware>();
            middleware.Setup(x => x.InvokeAsync(It.IsAny<IHttpFunctionContext>()))
                      .ThrowsAsync(new BadRequestException("oh no"));
            instance.Use(middleware.Object);
            var request = new Mock<HttpRequest>();
            var result = await instance.RunAsync(request.Object);
            var objectResult = result.ShouldBeOfType<BadRequestObjectResult>();
            objectResult.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "RunAsync should return expected response when no middleware is configured")]
        public async Task RunAsync_NoMiddleware_CustomExceptionHandler()
        {
            BadRequestObjectResult handlerResult = new BadRequestObjectResult("oh no");
            Func<Exception, IHttpFunctionContext, Task<IActionResult>> exceptionHandler = (Exception ex, IHttpFunctionContext context) =>
            {
                return Task.FromResult<IActionResult>(handlerResult);
            };

            var instance = this.CreateInstance();
            instance.ExceptionHandler = exceptionHandler;
            var request = new Mock<HttpRequest>();
            var result = await instance.RunAsync(request.Object);
            result.ShouldBe(handlerResult);
        }


        [Fact(DisplayName = "RunAsync should return expected response")]
        public async Task RunAsync()
        {
            OkObjectResult middlewareResponse = new OkObjectResult("hello");

            var instance = this.CreateInstance();
            var middleware = new DummyMiddleware()
            {
                Response = middlewareResponse
            };
            instance.Use(middleware);
            var request = new Mock<HttpRequest>();
            var result = await instance.RunAsync(request.Object);
            result.ShouldBe(middlewareResponse);
        }

        private MiddlewarePipeline CreateInstance()
        {
            return new MiddlewarePipeline();
        }
    }

    class DummyMiddleware : IHttpMiddleware
    {
        public IHttpMiddleware Next { get; set; }

        public IActionResult Response { get; set; }

        public Task InvokeAsync(IHttpFunctionContext context)
        {
            context.Response = this.Response;
            return Task.CompletedTask;
        }
    }
}
