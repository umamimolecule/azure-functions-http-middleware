using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using Xunit;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class CorrelationIdMiddlewareTests
    {
        private const string correlationIdHeader1 = "a";

        private const string correlationIdHeader2 = "b";

        public CorrelationIdMiddlewareTests()
        {
            var types = typeof(StatusCodeResult).Assembly.GetExportedTypes();
            var actionResults = types.Where(x => x.GetInterfaces().Contains(typeof(IActionResult))).ToArray();
        }

        [Fact(DisplayName = "Should invoke next middleware")]
        public async Task InvokeNextMiddleware()
        {
            HeaderDictionary headers = new HeaderDictionary
            {
                { "c", "abc123" }
            };

            var context = new ContextBuilder().AddRequestHeaders(headers)
                                              .Build();

            var instance = this.CreateInstance();

            var nextMiddleWare = new Mock<HttpMiddleware>();
            instance.Next = nextMiddleWare.Object;

            await instance.InvokeAsync(context);
            nextMiddleWare.Verify(x => x.InvokeAsync(context), Times.Once);
        }

        [Fact(DisplayName = "Should generate correlation identifier when no matching headers are found")]
        public async Task NoMatchingCorrelationHeaders()
        {
            HeaderDictionary headers = new HeaderDictionary
            {
                { "c", "abc123" }
            };

            var context = new ContextBuilder().AddRequestHeaders(headers)
                                              .Build();

            var instance = this.CreateInstance();
            await instance.InvokeAsync(context);
            context.TraceIdentifier.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact(DisplayName = "Should use supplied correlation identifier")]
        public async Task FirstMatchingCorrelationHeader()
        {
            HeaderDictionary headers = new HeaderDictionary
            {
                { correlationIdHeader1, "abc123" },
                { correlationIdHeader2, "abc124" }
            };

            var context = new ContextBuilder().AddRequestHeaders(headers)
                                              .Build();

            var instance = this.CreateInstance();
            await instance.InvokeAsync(context);
            context.TraceIdentifier.ShouldBe("abc123");
        }

        [Fact(DisplayName = "Should use supplied correlation identifier")]
        public async Task SecondMatchingCorrelationHeaders()
        {
            HeaderDictionary headers = new HeaderDictionary
            {
                { correlationIdHeader1, "" },
                { correlationIdHeader2, "abc124" }
            };

            var context = new ContextBuilder().AddRequestHeaders(headers)
                                              .Build();
            
            var instance = this.CreateInstance();
            await instance.InvokeAsync(context);
            context.TraceIdentifier.ShouldBe("abc124");
        }

        private CorrelationIdMiddleware CreateInstance()
        {
            return new CorrelationIdMiddleware(new string[] { correlationIdHeader1, correlationIdHeader2 });
        }
    }
}
