using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Moq;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class BodyModelValidationMiddlewareTests
    {
        [Fact(DisplayName = "InvokeAsync should correctly set the Body item for the context")]
        public async Task InvokeAsync()
        {
            var body = new Body()
            {
                Id = "1",
                Child = new Child()
                {
                    Id = "2",
                    Name = "Fred"
                }
            };

            var context = this.CreateContext(body);
            var instance = this.CreateInstance<Body>();
            await instance.InvokeAsync(context);

            context.Items["Body"].ShouldNotBeNull();
            var model = context.Items["Body"].ShouldBeOfType<Body>();
            model.Id.ShouldBe("1");
            model.Child.ShouldNotBeNull();
            model.Child.Id.ShouldBe("2");
            model.Child.Name.ShouldBe("Fred");
        }

        [Fact(DisplayName = "InvokeAsync should correctly call the next middleware")]
        public async Task InvokeAsync_CallsNextMiddleware()
        {
            var body = new Body()
            {
                Id = "1",
                Child = new Child()
                {
                    Id = "2",
                    Name = "Fred"
                }
            };

            var context = this.CreateContext(body);
            var instance = this.CreateInstance<Body>();

            var nextMiddleware = new Mock<IHttpMiddleware>();
            instance.Next = nextMiddleware.Object;
            await instance.InvokeAsync(context);

            nextMiddleware.Verify(x => x.InvokeAsync(It.IsAny<HttpContext>()), Times.Once);
        }

        [Theory(DisplayName = "InvokeAsync should throw the expected exception when a required property is missing for the default response handler")]
        [InlineData(null)]
        [InlineData("")]
        public async Task InvokeAsync_Fail_MissingRequiredProperties_DefaultResponseHandler(string id)
        {
            var body = new Body()
            {
                Id = null,
                Child = new Child()
                {
                    Id = id,
                    Name = "Fred"
                }
            };

            var context = this.CreateContext(body);
            var instance = this.CreateInstance<Body>();

            await instance.InvokeAsync(context);

            context.Response.StatusCode.ShouldBe(400);
            context.Response.Body.Position = 0;
            var contents = context.Response.Body.ReadAsString();
            var response = JsonConvert.DeserializeObject<ErrorResponse>(contents);
            response.ShouldNotBeNull();
            response.CorrelationId.ShouldNotBeNullOrWhiteSpace();
            response.Error.ShouldNotBeNull();
            response.Error.Code.ShouldBe("INVALID_BODY");
            response.Error.Message.ShouldContain("The Id field is required.");
        }

        [Theory(DisplayName = "InvokeAsync should throw the expected exception when a required property is missing for a custom response handler")]
        [InlineData(null)]
        [InlineData("")]
        public async Task InvokeAsync_Fail_MissingRequiredProperties_CustomResponseHandler(string id)
        {
            var body = new Body()
            {
                Id = null,
                Child = new Child()
                {
                    Id = id,
                    Name = "Fred"
                }
            };

            static IActionResult handler(HttpContext context, ModelValidationResult validationResult)
            {
                CustomErrorResponse response = new CustomErrorResponse()
                {
                    CustomErrorMessage = "Custom error: The Id field is required",
                };

                return new ObjectResult(response)
                {
                    StatusCode = 409,
                };
            }

            var context = this.CreateContext(body);
            var instance = this.CreateInstance<Body>();
            instance.HandleValidationFailure = handler;

            await instance.InvokeAsync(context);

            context.Response.StatusCode.ShouldBe(409);
            context.Response.Body.Position = 0;
            var contents = context.Response.Body.ReadAsString();
            var response = JsonConvert.DeserializeObject<CustomErrorResponse>(contents);
            response.ShouldNotBeNull();
            response.CustomErrorMessage.ShouldBe("Custom error: The Id field is required");
        }

        private BodyModelValidationMiddleware<T> CreateInstance<T>()
            where T : new()
        {
            return new BodyModelValidationMiddleware<T>();
        }

        private HttpContext CreateContext<T>(T body)
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
                                       .AddJsonBody(body)
                                       .Build();
        }

        class Body
        {
            [Required]
            public string Id { get; set; }

            public Child Child { get; set; }
        }

        class Child
        {
            [Required]
            public string Id { get; set; }

            public string Name { get; set; }
        }

        class CustomErrorResponse
        {
            public string CustomErrorMessage { get; set; }
        }
    }
}
