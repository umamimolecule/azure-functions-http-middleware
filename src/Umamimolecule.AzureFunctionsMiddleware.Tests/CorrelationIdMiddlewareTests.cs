using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class CorrelationIdMiddlewareTests
    {
        private const string correlationIdHeader1 = "a";

        private const string correlationIdHeader2 = "b";

        [Fact(DisplayName = "Should invoke next middleware")]
        public async Task InvokeNextMiddleware()
        {
            Dictionary<string, StringValues> headers = new Dictionary<string, StringValues>()
            {
                { "c", "abc123" }
            };

            var context = this.CreateContext(headers);
            var instance = this.CreateInstance();

            var nextMiddleWare = new Mock<HttpMiddleware>();
            instance.Next = nextMiddleWare.Object;

            await instance.InvokeAsync(context);
            nextMiddleWare.Verify(x => x.InvokeAsync(context), Times.Once);
        }

        [Fact(DisplayName = "Should generate correlation identifier when no matching headers are found")]
        public async Task NoMatchingCorrelationHeaders()
        {
            Dictionary<string, StringValues> headers = new Dictionary<string, StringValues>()
            {
                { "c", "abc123" }
            };

            var context = this.CreateContext(headers);
            var instance = this.CreateInstance();
            await instance.InvokeAsync(context);
            context.CorrelationId.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact(DisplayName = "Should use supplied correlation identifier")]
        public async Task FirstMatchingCorrelationHeader()
        {
            Dictionary<string, StringValues> headers = new Dictionary<string, StringValues>()
            {
                { correlationIdHeader1, "abc123" },
                { correlationIdHeader2, "abc124" }
            };

            var context = this.CreateContext(headers);
            var instance = this.CreateInstance();
            await instance.InvokeAsync(context);
            context.CorrelationId.ShouldBe("abc123");
        }

        [Fact(DisplayName = "Should use supplied correlation identifier")]
        public async Task SecondMatchingCorrelationHeaders()
        {
            Dictionary<string, StringValues> headers = new Dictionary<string, StringValues>()
            {
                { correlationIdHeader1, "" },
                { correlationIdHeader2, "abc124" }
            };

            var context = this.CreateContext(headers);
            var instance = this.CreateInstance();
            await instance.InvokeAsync(context);
            context.CorrelationId.ShouldBe("abc124");
        }

        private CorrelationIdMiddleware CreateInstance()
        {
            return new CorrelationIdMiddleware(new string[] { correlationIdHeader1, correlationIdHeader2 });
        }

        private IHttpFunctionContext CreateContext(Dictionary<string, StringValues> headers)
        {
            var request = new Mock<HttpRequest>();
            request.Setup(x => x.Headers)
                   .Returns(new HeaderDictionary(headers));

            return new HttpFunctionContext()
            {
                Request = request.Object
            };
        }
    }
}
