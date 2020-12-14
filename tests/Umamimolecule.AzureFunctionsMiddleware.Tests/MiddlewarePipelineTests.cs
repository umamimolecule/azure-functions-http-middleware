using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Moq;
using Shouldly;
using Xunit;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class MiddlewarePipelineTests
    {
        private readonly HttpContext context;
        private readonly HttpContextAccessor contextAccessor;
        private readonly Mock<ILoggerFactory> loggerFactory = new Mock<ILoggerFactory>();
        private readonly Logger logger = new Logger();

        public MiddlewarePipelineTests()
        {
            this.loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
                  .Returns(this.logger);

            ServiceCollection c = new ServiceCollection();
            c.AddMvcCore().AddJsonFormatters();
            c.AddOptions();

            c.AddTransient<ILoggerFactory>((IServiceProvider p) => this.loggerFactory.Object);
            c.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            var serviceProvider = c.BuildServiceProvider();
            //var serviceProvider = new DefaultServiceProvider();

            this.context = new ContextBuilder().AddServiceProvider(serviceProvider)
                                               .Accepts("application/json")
                                               .Build();
            this.contextAccessor = new HttpContextAccessor(this.context);
        }

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
            var exception = await Should.ThrowAsync<MiddlewarePipelineException>(() => instance.RunAsync());
        }

        //[Fact(DisplayName = "RunAsync should return expected response when last middleware does not return a response")]
        //public async Task RunAsync_LastMiddlewareNoResponse_NoCustomExceptionHandler()
        //{
        //    var instance = this.CreateInstance();
        //    var middleware = new Mock<IHttpMiddleware>();
        //    instance.Use(middleware.Object);
        //    var result = await instance.RunAsync();
        //    var httpResponseResult = result.ShouldBeOfType<HttpResponseResult>();
        //    await httpResponseResult.ExecuteResultAsync(new ActionContext(this.context, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()));
        //    this.context.Response.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
        //}

        [Fact(DisplayName = "RunAsync should return expected response when middleware throws BadRequestException")]
        public async Task RunAsync_MiddlewareThrowsBadRequestException_NoExceptionHandler()
        {
            var instance = this.CreateInstance();
            var middleware = new Mock<IHttpMiddleware>();
            middleware.Setup(x => x.InvokeAsync(It.IsAny<HttpContext>()))
                      .ThrowsAsync(new BadRequestException("oh no"));
            instance.Use(middleware.Object);
            var exception = await Should.ThrowAsync<BadRequestException>(() => instance.RunAsync());
        }

        [Fact(DisplayName = "RunAsync should return expected response")]
        public async Task RunAsync()
        {
            OkObjectResult middlewareResponse = new OkObjectResult("hello");

            var instance = this.CreateInstance();
            var middleware = new FunctionMiddleware((HttpContext context) =>
            {
                var obj = new
                {
                    message = "hello"
                };

                return Task.FromResult<IActionResult>(new OkObjectResult(obj));
            });

            instance.Use(middleware);
            var result = await instance.RunAsync();
            var httpResponseResult = result.ShouldBeOfType<HttpResponseResult>();
            await httpResponseResult.ExecuteResultAsync(new ActionContext() { HttpContext = this.context });
            this.context.Response.StatusCode.ShouldBe((int)HttpStatusCode.OK);
            //var content = this.context.Response.result.Content.ReadAsStringAsync();
            //content.ShouldBe("hello");
        }

        [Fact(DisplayName = "New should create a new pipeline with the same properties as the original pipeline.")]
        public void New()
        {
            var instance = this.CreateInstance();
            var result = instance.New().ShouldBeOfType<MiddlewarePipeline>();

            var httpContextAccessorField = typeof(MiddlewarePipeline).GetField("httpContextAccessor", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var originalHttpContextAccessor = (IHttpContextAccessor)httpContextAccessorField.GetValue(instance);
            var newHttpContextAccessor = (IHttpContextAccessor)httpContextAccessorField.GetValue(result);
            originalHttpContextAccessor.ShouldBeSameAs(newHttpContextAccessor);
        }

        [Fact(DisplayName = "UseWhen should execute the branch when the predicate returns true")]
        public async Task UseWhen_ConditionTrue()
        {
            var instance = this.CreateInstance();
            DummyLogger logger = new DummyLogger();
            instance.UseWhen(ctx => true,
                             p => p.Use(new LogMiddleware(logger, "a1"))
                                   .Use(new LogMiddleware(logger, "a2")));
            instance.Use(new LogMiddleware(logger, "b1"))
                    .Use(new LogMiddleware(logger, "b2"));

            var result = await instance.RunAsync();
            logger.Data.Count.ShouldBe(4);
            logger.Data.ToArray().ShouldBe(new string[] { "a1", "a2", "b1", "b2" });
        }

        [Fact(DisplayName = "UseWhen should not execute the branch when the predicate returns false")]
        public async Task UseWhen_ConditionFalse()
        {
            var instance = this.CreateInstance();
            DummyLogger logger = new DummyLogger();
            instance.UseWhen(ctx => false,
                             p => p.Use(new LogMiddleware(logger, "a1"))
                                   .Use(new LogMiddleware(logger, "a2")));
            instance.Use(new LogMiddleware(logger, "b1"))
                    .Use(new LogMiddleware(logger, "b2"));

            var result = await instance.RunAsync();
            logger.Data.Count.ShouldBe(2);
            logger.Data.ToArray().ShouldBe(new string[] { "b1", "b2" });
        }

        [Fact(DisplayName = "MapWhen should execute the branch when the predicate returns true")]
        public async Task MapWhen_ConditionTrue()
        {
            var instance = this.CreateInstance();
            DummyLogger logger = new DummyLogger();
            instance.MapWhen(ctx => true,
                             p => p.Use(new LogMiddleware(logger, "a1"))
                                   .Use(new LogMiddleware(logger, "a2")));
            instance.Use(new LogMiddleware(logger, "b1"))
                    .Use(new LogMiddleware(logger, "b2"));

            var result = await instance.RunAsync();
            logger.Data.Count.ShouldBe(2);
            logger.Data.ToArray().ShouldBe(new string[] { "a1", "a2" });
        }

        [Fact(DisplayName = "MapWhen should not execute the branch when the predicate returns false")]
        public async Task MapWhen_ConditionFalse()
        {
            var instance = this.CreateInstance();
            DummyLogger logger = new DummyLogger();
            instance.MapWhen(ctx => false,
                             p => p.Use(new LogMiddleware(logger, "a1"))
                                   .Use(new LogMiddleware(logger, "a2")));
            instance.Use(new LogMiddleware(logger, "b1"))
                    .Use(new LogMiddleware(logger, "b2"));

            var result = await instance.RunAsync();
            logger.Data.Count.ShouldBe(2);
            logger.Data.ToArray().ShouldBe(new string[] { "b1", "b2" });
        }

        private MiddlewarePipeline CreateInstance()
        {
            return new MiddlewarePipeline(this.contextAccessor);
        }

        class DummyLogger
        {
            public DummyLogger()
            {
                this.Data = new List<string>();
            }

            public List<string> Data { get; private set; }

            public void Log(string message)
            {
                this.Data.Add(message);
            }
        }

        class LogMiddleware : HttpMiddleware
        {
            private readonly DummyLogger logger;
            private readonly string id;

            public LogMiddleware(DummyLogger logger, string id)
            {
                this.logger = logger;
                this.id = id;
            }

            public override async Task InvokeAsync(HttpContext context)
            {
                this.logger.Log(this.id);

                if (this.Next != null)
                {
                    await this.Next.InvokeAsync(context);
                }
            }
        }
    }
}
