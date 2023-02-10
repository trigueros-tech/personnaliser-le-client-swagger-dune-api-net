using Api.Infrastructure.Authorization;
using Api.Infrastructure.Swagger;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(OAuth2IntrospectionDefaults.AuthenticationScheme)
                .AddOAuth2Introspection(options =>
                {
                    options.Authority = "http://localhost:8080/auth/realms/master/";
                    options.ClientId = "api-rest";
                    options.ClientSecret = "695e91bd-dbb0-4194-96ab-67a2a623ff13";
                });

            var authorizationPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser() // Require at least an authenticated user
                .AddRequirements(new CheckAuthorizationRequirement()) // do our custom verifications here
                .Build();

            services.AddAuthorization(options => options.DefaultPolicy = authorizationPolicy);

            services.AddScoped<IAuthorizationHandler, CheckAuthorizationRequirementHandler>();

            services.AddControllers(o => o.Filters.Add(new AuthorizeFilter()));

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen();

            // For DEV only.
            services.AddCors(options =>
                options.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1");
                    c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
                    c.OAuthClientId("swagger-client");
                    c.OAuthScopes("openid", "profile");
                });
            }

            app.UseHttpsRedirection();

            app.UseCors();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}