using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class DefaultServiceProvider : IServiceProvider
    {
        private readonly Mock<ILoggerFactory> loggerFactory = new Mock<ILoggerFactory>();
        private readonly Logger logger = new Logger();

        private Dictionary<Type, object> registeredServices = new Dictionary<Type, object>();

        public DefaultServiceProvider()
        {
            this.loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
                              .Returns(this.logger);

            this.registeredServices.Add(typeof(ILoggerFactory), this.loggerFactory.Object);
        }

        public object GetService(Type serviceType)
        {
            if (this.registeredServices.TryGetValue(serviceType, out object service))
            {
                return service;
            }

            return null;
        }
    }
}
