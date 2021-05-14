using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using Xunit;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class FunctionMiddlewareTests
    {
        private readonly Mock<HttpContext> context = new Mock<HttpContext>();

        [Fact(DisplayName = "Creating new instance with null function should throw expected exception")]
        public void NullFuncShouldThrowExpectedException()
        {
            var exception = ShouldThrowExtensions.ShouldThrow<ArgumentNullException>(() => new FunctionMiddleware(null));
            exception.ParamName.ShouldBe("func");
        }

        [Fact(DisplayName = "Invoking the middleware should call the function.")]
        public async Task FunctionShouldBeCalledOnInvoke()
        {
            Mock<IActionResult> result = new Mock<IActionResult>();

            int callCount = 0;
            Task<IActionResult> func(HttpContext ctx)
            {
                callCount++;
                return Task.FromResult<IActionResult>(result.Object);
            }

            FunctionMiddleware instance = new FunctionMiddleware(func);
            await instance.InvokeAsync(context.Object);
            callCount.ShouldBe(1);
        }
    }
}
