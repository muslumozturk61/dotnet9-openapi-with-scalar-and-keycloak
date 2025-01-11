using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace ScalarSample.Api
{
    internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
            if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
            {
                var securitySchemes = new Dictionary<string, OpenApiSecurityScheme>
                {
                    ["Keycloak"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            Implicit = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri("http://localhost:8080/realms/sample-realm/protocol/openid-connect/auth"),
                                Scopes = new Dictionary<string, string>
                               {
                                   { "openid", "openid" },
                                   { "profile", "profile" }
                               }
                            }
                        }
                    },
                    ["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer", // "bearer" refers to the header name here
                        In = ParameterLocation.Header,
                        BearerFormat = "Json Web Token"
                    }
                };


                var securityRequirement = new OpenApiSecurityRequirement
               {
                   {
                       new OpenApiSecurityScheme
                       {
                           Reference = new OpenApiReference
                           {
                               Id = "Keycloak",
                               Type = ReferenceType.SecurityScheme
                           },
                           In = ParameterLocation.Header,
                           Name = "Bearer",
                           Scheme = "Bearer"
                       },
                       []
                   },
                   {
                       new OpenApiSecurityScheme
                       {
                           Reference = new OpenApiReference
                           {
                               Id = "Bearer",
                               Type = ReferenceType.SecurityScheme
                           },
                           In = ParameterLocation.Header,
                           Name = "Bearer",
                           Scheme = "Bearer"
                       },
                       []
                   }
               };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes = securitySchemes;
                //document.SecurityRequirements.Add(securityRequirement);

            }
            //document.Info = new()
            //{
            //    Title = "ScalarSample API Scalar UI",
            //    Version = "v1",
            //    Description = "API for ScalarSample.",
            //    Contact = new OpenApiContact
            //    {
            //        Url = new Uri("http://www.muslumozturk.com"),
            //        Email = "muslum.ozturk@hotmail.com",
            //        Name = "Müslüm ÖZTÜRK"
            //    }
            //};
        }
    }
}
