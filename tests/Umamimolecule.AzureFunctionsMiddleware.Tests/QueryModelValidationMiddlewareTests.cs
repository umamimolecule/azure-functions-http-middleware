using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class QueryModelValidationMiddlewareTests
    {
        [Fact(DisplayName = "InvokeAsync should correctly set the Query item property for the context")]
        public async Task InvokeAsync()
        {
            var queryParameters = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "id", "1" },
                { "description", "hello" }
            });

            var context = this.CreateContext(queryParameters);

            var instance = this.CreateInstance<QueryParameters>();
            await instance.InvokeAsync(context);

            context.Items["Query"].ShouldNotBeNull();
            var model = context.Items["Query"].ShouldBeOfType<QueryParameters>();
            model.Id.ShouldBe("1");
            model.Description.ShouldBe("hello");
        }

        [Fact(DisplayName = "InvokeAsync should correctly call the next middleware")]
        public async Task InvokeAsync_CallsNextMiddleware()
        {
            var queryParameters = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "id", "1" },
                { "description", "hello" }
            });

            var context = this.CreateContext(queryParameters);

            var instance = this.CreateInstance<QueryParameters>();

            var nextMiddleware = new Mock<IHttpMiddleware>();
            instance.Next = nextMiddleware.Object;
            await instance.InvokeAsync(context);

            nextMiddleware.Verify(x => x.InvokeAsync(It.IsAny<HttpContext>()), Times.Once);
        }

        [Theory(DisplayName = "InvokeAsync should throw the expected exception when a require property is missing")]
        [InlineData(null)]
        [InlineData("")]
        public async Task InvokeAsync_Fail_MissingRequiredProperties(string id)
        {
            var queryParameters = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "id", id },
                { "description", "hello" }
            });

            var context = this.CreateContext(queryParameters);

            var instance = this.CreateInstance<QueryParameters>();
            await instance.InvokeAsync(context);
            context.Response.StatusCode.ShouldBe(400);
            context.Response.Body.Position = 0;
            var contents = context.Response.Body.ReadAsString();
            var response = JsonConvert.DeserializeObject<ErrorResponse>(contents);
            response.ShouldNotBeNull();
            response.CorrelationId.ShouldNotBeNullOrWhiteSpace();
            response.Error.ShouldNotBeNull();
            response.Error.Code.ShouldBe("INVALID_QUERY_PARAMETERS");
            response.Error.Message.ShouldBe("The Id field is required.");
        }

        [Theory(DisplayName = "InvokeAsync should throw the expected exception when a required property is missing for a custom response handler")]
        [InlineData(null)]
        [InlineData("")]
        public async Task InvokeAsync_Fail_MissingRequiredProperties_CustomResponseHandler(string id)
        {
            var queryParameters = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "id", id },
                { "description", "hello" }
            });

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

            var context = this.CreateContext(queryParameters);
            var instance = this.CreateInstance<QueryParameters>();
            instance.HandleValidationFailure = handler;

            await instance.InvokeAsync(context);

            context.Response.StatusCode.ShouldBe(409);
            context.Response.Body.Position = 0;
            var contents = context.Response.Body.ReadAsString();
            var response = JsonConvert.DeserializeObject<CustomErrorResponse>(contents);
            response.ShouldNotBeNull();
            response.CustomErrorMessage.ShouldBe("Custom error: The Id field is required");
        }

        private QueryModelValidationMiddleware<T> CreateInstance<T>()
            where T : new()
        {
            return new QueryModelValidationMiddleware<T>();
        }

        private HttpContext CreateContext(IQueryCollection queryParameters)
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
                                       .AddQuery(queryParameters)
                                       .Build();
        }

        class QueryParameters
        {
            [Required]
            public string Id { get; set; }

            public string Description { get; set; }
        }

        class CustomErrorResponse
        {
            public string CustomErrorMessage { get; set; }
        }

    }
}
