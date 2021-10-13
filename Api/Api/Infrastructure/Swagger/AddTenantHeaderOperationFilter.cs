using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Infrastructure.Swagger
{
    public class AddTenantHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }
            
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-tenant",
                In = ParameterLocation.Header,
                Description = "Tenant key",
                Required = true
            });
        }
    }
}