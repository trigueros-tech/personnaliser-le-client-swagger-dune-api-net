using System;
using System.Collections.Generic;
using System.Net.Http;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Infrastructure.Swagger
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly HttpClient _httpClient;

        public ConfigureSwaggerOptions(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void Configure(SwaggerGenOptions options)
        {
            options.DescribeAllParametersInCamelCase();
            options.CustomSchemaIds(x => x.FullName);
            options.SwaggerDoc("v1", CreateOpenApiInfo());

            options.OperationFilter<AuthorizeOperationFilter>();
            options.AddSecurityDefinition("oauth2", CreateSecurityDefinition());
            options.AddSecurityRequirement(CreateSecurityRequirement());
            
            options.OperationFilter<AddTenantHeaderOperationFilter>();
        }

        public OpenApiSecurityScheme CreateSecurityDefinition()
        {
            var discoveryDocument = GetDiscoveryDocument();
            return new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(discoveryDocument.AuthorizeEndpoint),
                        TokenUrl = new Uri(discoveryDocument.TokenEndpoint),
                        Scopes = new Dictionary<string, string>
                        {
                            {"openid", "default scope"},
                            {"profile", "user profile"}
                        }
                    }
                },
                Description = "Keycloak"
            };
        }

        public DiscoveryDocumentResponse GetDiscoveryDocument()
        {
            return _httpClient
                .GetDiscoveryDocumentAsync("http://localhost:8080/auth/realms/master")
                .GetAwaiter()
                .GetResult();
        }

        public OpenApiSecurityRequirement CreateSecurityRequirement()
        {
            return new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "oauth2"},
                    },
                    new[] {"openid", "profile"}
                }
            };
        }
        public OpenApiInfo CreateOpenApiInfo()
        {
            return new OpenApiInfo
            {
                Title = "My Api",
                Version = "v1",
                Description = "My Api"
            };
        }
    }
}