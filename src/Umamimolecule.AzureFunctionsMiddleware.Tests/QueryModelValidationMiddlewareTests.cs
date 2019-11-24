using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Moq;
using Shouldly;
using Xunit;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class QueryModelValidationMiddlewareTests
    {
        [Fact(DisplayName = "InvokeAsync should correctly set the QueryModel property for the context")]
        public async Task InvokeAsync()
        {
            var queryParameters = new Dictionary<string, StringValues>()
            {
                { "id", "1" },
                { "description", "hello" }
            };

            var context = this.CreateContext(queryParameters);
            
            var instance = this.CreateInstance<QueryParameters>();
            await instance.InvokeAsync(context.Object);

            context.VerifySet(x => x.QueryModel = It.Is<QueryParameters>(q => q.Id == "1" && q.Description == "hello"), Times.Once);
        }

        [Fact(DisplayName = "InvokeAsync should correctly call the next middleware")]
        public async Task InvokeAsync_CallsNextMiddleware()
        {
            var queryParameters = new Dictionary<string, StringValues>()
            {
                { "id", "1" },
                { "description", "hello" }
            };

            var context = this.CreateContext(queryParameters);

            var instance = this.CreateInstance<QueryParameters>();

            var nextMiddleware = new Mock<IHttpMiddleware>();
            instance.Next = nextMiddleware.Object;
            await instance.InvokeAsync(context.Object);

            nextMiddleware.Verify(x => x.InvokeAsync(It.IsAny<IHttpFunctionContext>()), Times.Once);
        }

        [Theory(DisplayName = "InvokeAsync should throw the expected exception when a require property is missing")]
        [InlineData(null)]
        [InlineData("")]
        public async Task InvokeAsync_Fail_MissingRequiredProperties(string id)
        {
            var queryParameters = new Dictionary<string, StringValues>()
            {
                { "id", id },
                { "description", "hello" }
            };

            var context = this.CreateContext(queryParameters);

            var instance = this.CreateInstance<QueryParameters>();
            var exception = await ShouldThrowAsyncExtensions.ShouldThrowAsync<BadRequestException>(() => instance.InvokeAsync(context.Object));
            exception.Message.ShouldContain("The Id field is required");
        }

        private QueryModelValidationMiddleware<T> CreateInstance<T>()
            where T : new()
        {
            return new QueryModelValidationMiddleware<T>();
        }

        private Mock<IHttpFunctionContext> CreateContext(Dictionary<string, StringValues> queryParameters)
        {
            var context = new Mock<IHttpFunctionContext>();
            
            var request = new Mock<HttpRequest>();
            request.Setup(x => x.Query)
                   .Returns(new QueryCollection(queryParameters));

            context.Setup(x => x.Request)
                   .Returns(request.Object);

            return context;
        }

        class QueryParameters
        {
            [Required]
            public string Id { get; set; }

            public string Description { get; set; }
        }
    }
}
