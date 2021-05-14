using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Shouldly;
using Xunit;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class ConditionalMiddlewareTests
    {
        private readonly Mock<HttpContext> context = new Mock<HttpContext>();

        private readonly Mock<IMiddlewarePipeline> pipeline = new Mock<IMiddlewarePipeline>();

        private readonly Mock<IMiddlewarePipeline> branch = new Mock<IMiddlewarePipeline>();

        private readonly Func<HttpContext, bool> condition;

        private readonly Mock<IHttpMiddleware> next = new Mock<IHttpMiddleware>();

        private readonly Action<IMiddlewarePipeline> configure;

        private bool conditionResult = true;

        private bool configured = false;

        public ConditionalMiddlewareTests()
        {
            this.condition = (context) => this.conditionResult;
            this.configure = (pipeline) => { this.configured = true; };
            this.pipeline.Setup(x => x.New()).Returns(branch.Object);
        }

        [Fact(DisplayName = "Creating new instance with null pipeline should throw expected exception")]
        public void NullPipelineShouldThrowExpectedException()
        {
            var exception = ShouldThrowExtensions.ShouldThrow<ArgumentNullException>(() => new ConditionalMiddleware(null, condition, configure, true));
            exception.ParamName.ShouldBe("pipeline");
        }

        [Fact(DisplayName = "Creating new instance with null condition should throw expected exception")]
        public void NullConditionShouldThrowExpectedException()
        {
            var exception = ShouldThrowExtensions.ShouldThrow<ArgumentNullException>(() => new ConditionalMiddleware(pipeline.Object, null, configure, true));
            exception.ParamName.ShouldBe("condition");
        }

        [Fact(DisplayName = "Creating new instance with null configure should not throw exception")]
        public void NullConfigureShouldNotThrowExpectedException()
        {
            new ConditionalMiddleware(pipeline.Object, condition, null, true);
        }

        [Fact(DisplayName = "InvokeAsync should execute the branch when condition evaluates to true but not rejoin pipeline")]
        public async Task InvokeAsync_ConditionTrue_DoNotRejoinPipeline()
        {
            var instance = new ConditionalMiddleware(pipeline.Object, condition, configure, false)
            {
                Next = next.Object
            };
            await instance.InvokeAsync(this.context.Object);
            this.configured.ShouldBeTrue();
            this.branch.Verify(x => x.RunAsync(), Times.Once);
            this.next.Verify(x => x.InvokeAsync(this.context.Object), Times.Never);
        }

        [Fact(DisplayName = "InvokeAsync should execute the branch when condition evaluates to true and rejoin pipeline")]
        public async Task InvokeAsync_ConditionTrue_RejoinPipeline()
        {
            var instance = new ConditionalMiddleware(pipeline.Object, condition, configure, true)
            {
                Next = next.Object
            };
            await instance.InvokeAsync(this.context.Object);
            this.configured.ShouldBeTrue();
            this.branch.Verify(x => x.RunAsync(), Times.Once);
            this.next.Verify(x => x.InvokeAsync(this.context.Object), Times.Once);
        }

        [Fact(DisplayName = "InvokeAsync should not execute the branch when condition evaluates to false")]
        public async Task InvokeAsync_ConditionFalse_RejoinPipeline()
        {
            this.conditionResult = false;
            var instance = new ConditionalMiddleware(pipeline.Object, condition, configure, true)
            {
                Next = next.Object
            };
            await instance.InvokeAsync(this.context.Object);
            this.configured.ShouldBeFalse();
            this.branch.Verify(x => x.RunAsync(), Times.Never);
            this.next.Verify(x => x.InvokeAsync(this.context.Object), Times.Once);
        }
    }
}
