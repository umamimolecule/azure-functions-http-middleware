﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class BodyValidatorTests
    {
        [Fact(DisplayName = "Validation should succeed when supplied an empty instance without any required fields")]
        public void NoRequiredFieldsNoValuesSupplied()
        {
            List<ValidationResult> validationResults = new List<ValidationResult>();
            var result = RecursiveValidator.TryValidateObject(new BodySimple(), validationResults, true);
            result.ShouldBeTrue();
            validationResults.ShouldBeEmpty();
        }

        [Fact(DisplayName = "Validation should succeed when supplied an instance without any required fields")]
        public void NoRequiredFieldsAllValuesSupplied()
        {
            BodySimple body = new BodySimple()
            {
                Param1 = "value1",
                Param2 = "value2"
            };

            List<ValidationResult> validationResults = new List<ValidationResult>();
            var result = RecursiveValidator.TryValidateObject(body, validationResults, true);
            result.ShouldBeTrue();
            validationResults.ShouldBeEmpty();
        }

        [Theory(DisplayName = "Validation should fail when supplied an instance missing required fields")]
        [InlineData(null)]
        [InlineData("")]
        public void RequiredFieldsNotSuppliedOrEmpty(string param1)
        {
            BodyWithRequiredFields body = new BodyWithRequiredFields()
            {
                Param1 = param1,
                Param2 = "value2"
            };

            List<ValidationResult> validationResults = new List<ValidationResult>();
            var result = RecursiveValidator.TryValidateObject(body, validationResults, true);
            result.ShouldBeFalse();
            validationResults.ShouldContain(x => x.ErrorMessage.Contains("Param1 is required"));
        }

        [Fact(DisplayName = "Validation should succeed when supplied an instance containing all required fields")]
        public void RequiredFieldsAllValuesSupplied()
        {
            BodyWithRequiredFields body = new BodyWithRequiredFields()
            {
                Param1 = "value1",
                Param2 = "value2"
            };

            List<ValidationResult> validationResults = new List<ValidationResult>();
            var result = RecursiveValidator.TryValidateObject(body, validationResults, true);
            result.ShouldBeTrue();
            validationResults.ShouldBeEmpty();
        }

        [Fact(DisplayName = "Validation should fail when a nested collection field is required but is null")]
        public void NestedCollectionFieldMissingRequiredValue()
        {
            BodyWithNestedRequiredUrlField body = new BodyWithNestedRequiredUrlField()
            {
                UrlParam = new BodyWithUrlField()
                {
                    Param1 = "https://google.com"
                },
                Items = new BodyWithRequiredFields[]
                {
                    new BodyWithRequiredFields()
                    {
                        Param1 = null,
                        Param2 = "blah"
                    }
                }
            };

            List<ValidationResult> validationResults = new List<ValidationResult>();
            var result = RecursiveValidator.TryValidateObject(body, validationResults, true);
            result.ShouldBeFalse();
            validationResults.ShouldContain(x => x.MemberNames.Contains("Items[0].Param1") && x.ErrorMessage.Contains("Param1 is required"));
        }

        [Fact(DisplayName = "Validation should fail when a nested object field is required but is null")]
        public void NestedChildFieldMissingRequiredValue()
        {
            BodyWithNestedRequiredUrlField body = new BodyWithNestedRequiredUrlField()
            {
                UrlParam = new BodyWithUrlField()
                {
                    Param1 = null
                }                
            };

            List<ValidationResult> validationResults = new List<ValidationResult>();
            var result = RecursiveValidator.TryValidateObject(body, validationResults, true);
            result.ShouldBeFalse();
            validationResults.ShouldContain(x => x.MemberNames.Contains("UrlParam.Param1") && x.ErrorMessage.Contains("Param1 is required"));
        }

        [Fact(DisplayName = "Validation should fail when required readonly properties are null")]
        public void ShouldValidateReadOnlyProperties()
        {
            ClassWithRequiredReadonlyProperty body = new ClassWithRequiredReadonlyProperty()
            {
                Child = new ClassWithRequiredReadonlyProperty()
            };

            List<ValidationResult> validationResults = new List<ValidationResult>();
            var result = RecursiveValidator.TryValidateObject(body, validationResults, true);
            result.ShouldBeFalse();
            validationResults.ShouldContain(x => x.MemberNames.Contains("ReadOnlyProperty") && x.ErrorMessage.Contains("ReadOnlyProperty field is required"));
            validationResults.ShouldContain(x => x.MemberNames.Contains("Child.ReadOnlyProperty") && x.ErrorMessage.Contains("ReadOnlyProperty field is required"));
        }

        [Fact(DisplayName = "Should throw the expected exception when attempting to validate a null instance.")]
        public void ThrowsArgumentNullException()
        {
            List<ValidationResult> validationResults = new List<ValidationResult>();
            var exception = ShouldThrowExtensions.ShouldThrow<ArgumentNullException>(() => RecursiveValidator.TryValidateObject(null, validationResults, true));
            exception.ParamName.ShouldBe("instance");
        }

        private BodyModelValidationMiddleware<T> CreateInstance<T>() where T : new()
        {
            return new BodyModelValidationMiddleware<T>();
        }

        private Mock<HttpContext> CreateContext<T>(T body)
        {
            Mock<HttpContext> context = new Mock<HttpContext>();

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(JsonConvert.SerializeObject(body));
            writer.Flush();
            stream.Position = 0;

            Mock<HttpRequest> request = new Mock<HttpRequest>();
            request.Setup(x => x.Body)
                   .Returns(stream);

            context.Setup(x => x.Request)
                   .Returns(request.Object);

            return context;
        }

        class BodySimple
        {
            public string Param1 { get; set; }

            public string Param2 { get; set; }
        }

        class BodyWithRequiredFields
        {
            [Required(AllowEmptyStrings = false, ErrorMessage = "Param1 is required")]
            public string Param1 { get; set; }

            public string Param2 { get; set; }
        }

        class BodyWithUrlField
        {
            [Required(ErrorMessage = "Param1 is required")]
            [UrlValidation(ErrorMessage = "Should be a valid URL")]
            public string Param1 { get; set; }
        }

        class BodyWithNestedRequiredUrlField
        {
            [Required(ErrorMessage = "UrlParam is required")]
            public BodyWithUrlField UrlParam { get; set; }

            public BodyWithUrlField NonRequiredUrlParam { get; set; }

            public IEnumerable<BodyWithRequiredFields> Items { get; set; }
        }

        class UrlValidationAttribute : ValidationAttribute
        {
            /// <summary>
            /// Determines whether the specified value of the object is valid.
            /// </summary>
            /// <param name="value">The value of the object to validate.</param>
            /// <returns>true if the specified value is valid; otherwise, false.</returns>
            public override bool IsValid(object value)
            {
                if (value == null)
                {
                    return true;
                }

                return Uri.TryCreate(value.ToString(), UriKind.Absolute, out Uri _);
            }
        }

        class ClassWithRequiredReadonlyProperty
        {
            [Required]
            public string ReadOnlyProperty => null;

            public ClassWithRequiredReadonlyProperty Child { get; set; }
        }
    }
}
