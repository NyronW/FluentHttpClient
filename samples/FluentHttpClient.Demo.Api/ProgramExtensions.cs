using FluentHttpClient.Demo.Api.Features.Todo;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MinimalEndpoints;

namespace FluentHttpClient.Demo.Api
{
    public static class ProgramExtensions
    {
        public static WebApplicationBuilder ConfigureBuilder(this WebApplicationBuilder builder)
        {
            builder.Services.AddMinimalEndpoints();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "FluentHttpClient.Demo.Api API",
                    Version = "v1",
                    Description = "An API developed using MinimalEndpoint",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Author Name",
                        Url = new Uri("https://github.com/nyronw"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "{FluentHttpClient.Demo.Api} License",
                        Url = new Uri("https://example.com/license"),
                    }
                });

                c.OperationFilter<SecureSwaggerEndpointRequirementFilter>();

            // Set the comments path for the Swagger JSON and UI.
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory)
                    .Where(f => Path.GetExtension(f) == ".xml");

                foreach (var xmlFile in xmlFiles)
                {
                    c.IncludeXmlComments(xmlFile);
                }

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
            });

            builder.Services.AddSingleton<ITodoRepository, TodoRepository>();

            builder.Services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.RequireHttpsMetadata = false;         //False for local addresses, true ofcourse for live scenarios
                                options.Authority = "https://localhost:7094/";//IdentityServer URL
                                options.ApiName = "api1";
                    options.ApiSecret = "ScopeSecret";
                });

            builder.Services.AddAuthorization();

            return builder;
        }

        public static WebApplication ConfigureApplication(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "FluentHttpClient.Demo.Api API");
            });
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMinimalEndpoints(options =>
            {
                options.DefaultRoutePrefix = "/api/v1";
                options.DefaultGroupName = "v1";
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest, "application/problem+"));
            });

            return app;
        }
    }
}