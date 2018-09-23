using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BCC.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MultiPartFormBindingAttribute : Attribute
    {
        public Type BindingType { get; set; }

        public MultiPartFormBindingAttribute(Type bindingType)
        {
            BindingType = bindingType;
        }

        public class MultiPartFormBindingFilter : IOperationFilter
        {
            public void Apply(Operation operation, OperationFilterContext context)
            {
                var customBindingAttributes = context.MethodInfo.GetCustomAttributes(typeof(MultiPartFormBindingAttribute), true)
                    .Select(o => (MultiPartFormBindingAttribute)o);

                operation.Consumes = new []{"multipart/form-data"};

                foreach (var customBindingAttribute in customBindingAttributes)
                {
                    foreach (var property in customBindingAttribute.BindingType.GetProperties())
                    {
                        string parameterType = null;
                        string parameterFormat = null;

                        if (property.PropertyType == typeof(string))
                        {
                            parameterType = "string";
                        }

                        if (property.PropertyType == typeof(int))
                        {
                            parameterFormat = "int32";
                            parameterType = "integer";
                        }

                        if (property.PropertyType == typeof(long))
                        {
                            parameterFormat = "int64";
                            parameterType = "integer";
                        }

                        if (property.PropertyType == typeof(float))
                        {
                            parameterFormat = "float";
                            parameterType = "number";
                        }

                        if (property.PropertyType == typeof(double))
                        {
                            parameterFormat = "double";
                            parameterType = "number";
                        }

                        if (property.GetCustomAttributes(typeof(FormFileAttribute), true).Any())
                        {
                            parameterFormat = "binary";
                            parameterType = "file";
                        }

                        operation.Parameters.Add(new NonBodyParameter()
                        {
                            Name = property.Name,
                            In = "formData",
                            Required = property.GetCustomAttributes(typeof(RequiredAttribute), true).Any(),
                            Type = parameterType,
                            Format = parameterFormat
                        });
                    }
                }

            }
        }
    }
}
