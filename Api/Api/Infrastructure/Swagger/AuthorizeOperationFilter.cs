using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Infrastructure.Swagger
{
    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var authAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes<AuthorizeAttribute>(true)
                .Union(context.MethodInfo.GetCustomAttributes<AuthorizeAttribute>(true));

            if (authAttributes.Any())
            {
                operation.Responses.Add(StatusCodes.Status401Unauthorized.ToString(),
                    new OpenApiResponse {Description = nameof(HttpStatusCode.Unauthorized)});
                operation.Responses.Add(StatusCodes.Status403Forbidden.ToString(),
                    new OpenApiResponse {Description = nameof(HttpStatusCode.Forbidden)});

                operation.Security = new List<OpenApiSecurityRequirement>();

                var oauth2SecurityScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "OAuth2"},
                };

                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [oauth2SecurityScheme] = new[] {"OAuth2"}
                });
            }
        }
    }
}