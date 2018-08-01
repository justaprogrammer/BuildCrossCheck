using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MSBLOC.Web.Attributes;
using MSBLOC.Web.Tests.Util;
using NSubstitute;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Web.Tests.Attributes
{
    public class MultiPartFormBindingAttributeTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<MultiPartFormBindingAttributeTests> _logger;

        public MultiPartFormBindingAttributeTests(ITestOutputHelper testOutputHelper)
        {
            _logger = TestLogger.Create<MultiPartFormBindingAttributeTests>(testOutputHelper);
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ApplyMultiPartFormBindingTest()
        {
            var operationFilter = new MultiPartFormBindingAttribute.MultiPartFormBindingFilter();

            var operation = Substitute.For<Operation>();
            var methodInfo = Substitute.For<MethodInfo>();
            var context = new OperationFilterContext(null, null, methodInfo);

            var bindingAttributes = new object[] {new MultiPartFormBindingAttribute(typeof(BindTestPrimitives))};

            methodInfo.GetCustomAttributes(typeof(MultiPartFormBindingAttribute), true).Returns(bindingAttributes);
            operation.Parameters = new List<IParameter>();

            operationFilter.Apply(operation, context);

            operation.Consumes.Should().BeEquivalentTo("multipart/form-data");
            operation.Parameters.Should().BeEquivalentTo(new List<NonBodyParameter>
            {
                new NonBodyParameter()
                {
                    Name = nameof(BindTestPrimitives.Id),
                    In = "formData",
                    Required = true,
                    Type = "integer",
                    Format = "int32"
                },
                new NonBodyParameter()
                {
                    Name = nameof(BindTestPrimitives.Name),
                    In = "formData",
                    Type = "string"
                },
                new NonBodyParameter()
                {
                    Name = nameof(BindTestPrimitives.FloatingPointNumber),
                    In = "formData",
                    Type = "number",
                    Format = "float"
                },
                new NonBodyParameter()
                {
                    Name = nameof(BindTestPrimitives.DoubleNumber),
                    In = "formData",
                    Type = "number",
                    Format = "double"
                },
                new NonBodyParameter()
                {
                    Name = nameof(BindTestPrimitives.LongNumber),
                    In = "formData",
                    Type = "integer",
                    Format = "int64"
                }
            }, options => options.Including(p => p.Format).Including(p => p.Name).Including(p => p.In).Including(p => p.Type));
        }

        [Fact]
        public void ApplyMultiPartFormBindingWithFormFileTest()
        {
            var operationFilter = new MultiPartFormBindingAttribute.MultiPartFormBindingFilter();

            var operation = Substitute.For<Operation>();
            var methodInfo = Substitute.For<MethodInfo>();
            var context = new OperationFilterContext(null, null, methodInfo);

            var bindingAttributes = new object[] { new MultiPartFormBindingAttribute(typeof(BindTestFormFile)) };

            methodInfo.GetCustomAttributes(typeof(MultiPartFormBindingAttribute), true).Returns(bindingAttributes);
            operation.Parameters = new List<IParameter>();

            operationFilter.Apply(operation, context);

            operation.Consumes.Should().BeEquivalentTo("multipart/form-data");
            operation.Parameters.Should().BeEquivalentTo(new List<NonBodyParameter>
            {
                new NonBodyParameter()
                {
                    Name = nameof(BindTestFormFile.Id),
                    In = "formData",
                    Required = true,
                    Type = "integer",
                    Format = "int32"
                },
                new NonBodyParameter()
                {
                    Name = nameof(BindTestFormFile.Name),
                    In = "formData",
                    Type = "file",
                    Format = "binary"
                }
            }, options => options.Including(p => p.Format).Including(p => p.Name).Including(p => p.In).Including(p => p.Type));
        }

        private class BindTestPrimitives
        {
            [Required]
            public int Id { get; set; }
            public string Name { get; set; }
            public float FloatingPointNumber { get; set; }
            public double DoubleNumber { get; set; }
            public long LongNumber { get; set; }
        }

        private class BindTestFormFile
        {
            [Required]
            public int Id { get; set; }
            [FormFile]
            public string Name { get; set; }
        }

    }
}
