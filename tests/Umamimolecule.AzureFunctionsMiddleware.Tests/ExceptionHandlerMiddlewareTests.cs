using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Moq;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class ExceptionHandlerMiddlewareTests
    {
        [Fact(DisplayName = "InvokeAsync should throw the expected exception when Next middleware is not set")]
        public async Task InvokeAsync_ThrowsExceptionWhenNextIsNotSet()
        {
            ExceptionHandlerMiddleware middleware = new ExceptionHandlerMiddleware();
            var context = this.GetContext();
            var exception = await ShouldThrowAsyncExtensions.ShouldThrowAsync<MiddlewarePipelineException>(() => middleware.InvokeAsync(context));
            exception.Message.ShouldContain("must have a Next middleware");
        }

        [Fact(DisplayName = "InvokeAsync should succeed when no exceptions are thrown by the next middleware")]
        public async Task InvokeAsync_NoExceptionFromNextMiddleware()
        {
            ExceptionHandlerMiddleware middleware = new ExceptionHandlerMiddleware()
            {
                Next = new DummyMiddleware()
            };
            var context = this.GetContext();
            await middleware.InvokeAsync(context);
            context.Response.StatusCode.ShouldBe(202);
        }

        [Fact(DisplayName = "InvokeAsync should call LogExceptionAsync when an exception is thrown by the next middleware")]
        public async Task InvokeAsync_CallLogExceptionAsync()
        {
            Exception loggedException = null;
            ExceptionHandlerMiddleware middleware = new ExceptionHandlerMiddleware()
            {
                Next = new FaultyMiddleware(),
                LogExceptionAsync = (Exception e) =>
                {
                    loggedException = e;
                    return Task.CompletedTask;
                }
            };
            var context = this.GetContext();
            await middleware.InvokeAsync(context);
            var typedLoggedException = loggedException.ShouldBeOfType<ApplicationException>();
            typedLoggedException.Message.ShouldBe("oh no!");
        }

        [Fact(DisplayName = "InvokeAsync should set the expected response when no exception handler is defined")]
        public async Task InvokeAsync_NoExceptionHandler()
        {
            ExceptionHandlerMiddleware middleware = new ExceptionHandlerMiddleware()
            {
                Next = new FaultyMiddleware()
            };
            var context = this.GetContext();
            await middleware.InvokeAsync(context);
            context.Response.StatusCode.ShouldBe(500);
            context.Response.Body.Position = 0;
            var contents = context.Response.Body.ReadAsString();
            var response = JsonConvert.DeserializeObject<ErrorResponse>(contents);
            response.ShouldNotBeNull();
            response.CorrelationId.ShouldNotBeNullOrWhiteSpace();
            response.Error.ShouldNotBeNull();
            response.Error.Code.ShouldBe("INTERNAL_SERVER_ERROR");
            response.Error.Message.ShouldBe("An internal server error occurred");
        }

        [Fact(DisplayName = "InvokeAsync should set the expected response when the default exception handler is used")]
        public async Task InvokeAsync_DefaultExceptionHandler()
        {
            ExceptionHandlerMiddleware middleware = new ExceptionHandlerMiddleware()
            {
                Next = new FaultyMiddleware(),
                ExceptionHandler = ExceptionHandlerMiddleware.DefaultExceptionHandler
            };
            var context = this.GetContext();
            await middleware.InvokeAsync(context);
            context.Response.StatusCode.ShouldBe(500);
            context.Response.Body.Position = 0;
            var contents = context.Response.Body.ReadAsString();
            var response = JsonConvert.DeserializeObject<ErrorResponse>(contents);
            response.ShouldNotBeNull();
            response.CorrelationId.ShouldNotBeNullOrWhiteSpace();
            response.Error.ShouldNotBeNull();
            response.Error.Code.ShouldBe("INTERNAL_SERVER_ERROR");
            response.Error.Message.ShouldBe("An unexpected error occurred in the application");
        }

        [Fact(DisplayName = "InvokeAsync should set the expected response when the default exception handler is used")]
        public async Task InvokeAsync_DefaultExceptionHandler_BadRequest()
        {
            ExceptionHandlerMiddleware middleware = new ExceptionHandlerMiddleware()
            {
                Next = new BadRequestMiddleware(),
                ExceptionHandler = ExceptionHandlerMiddleware.DefaultExceptionHandler
            };
            var context = this.GetContext();
            await middleware.InvokeAsync(context);
            context.Response.StatusCode.ShouldBe(400);
            context.Response.Body.Position = 0;
            var contents = context.Response.Body.ReadAsString();
            var response = JsonConvert.DeserializeObject<ErrorResponse>(contents);
            response.ShouldNotBeNull();
            response.CorrelationId.ShouldNotBeNullOrWhiteSpace();
            response.Error.ShouldNotBeNull();
            response.Error.Code.ShouldBe("BAD_REQUEST");
            response.Error.Message.ShouldBe("oh no!");
        }

        private HttpContext GetContext()
        {
            Logger logger = new Logger();
            Mock<ILoggerFactory> loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
                         .Returns(logger);

            ServiceCollection services = new ServiceCollection();
            services.AddMvcCore().AddJsonFormatters();
            services.AddOptions();

            services.AddTransient<ILoggerFactory>((IServiceProvider p) => loggerFactory.Object);
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            var serviceProvider = services.BuildServiceProvider();

            return new ContextBuilder().AddServiceProvider(serviceProvider)
                                       .Build();
        }

        class DummyMiddleware : HttpMiddleware
        {
            public override Task InvokeAsync(HttpContext context)
            {
                context.Response.StatusCode = 202;
                return Task.CompletedTask;
            }
        }

        class FaultyMiddleware : HttpMiddleware
        {
            public override Task InvokeAsync(HttpContext context)
            {
                throw new ApplicationException("oh no!");
            }
        }

        class BadRequestMiddleware : HttpMiddleware
        {
            public override Task InvokeAsync(HttpContext context)
            {
                throw new BadRequestException("oh no!");
            }
        }
    }
}
