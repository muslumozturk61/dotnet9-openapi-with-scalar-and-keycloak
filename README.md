# .Net 9 OpenApi With Scalar and Keyclaok

.Net Core 9.0 Web API projesinde scalar ve keycloak kullanımı içerek örnek bir projedir.

Uygulama yapılandırması ve proje yapısı ile ilgili anlattımı ile ilgili videoyu [youtubeden](https://www.youtube.com/watch?v=M81Ve6gF_Bs) izleyebilirsiniz.


add Scalar.AspNetCore dependecy to ScalarSample.Api project

``` sh
dotnet add package Scalar.AspNetCore --version 1.2.74
```

in program.cs file

``` csharp
 builder.Services.AddOpenApi(options =>
 {
     options.AddDocumentTransformer((document, context, cancellationToken) =>
     {
         document.Info = new()
         {
             Title = "ScalarSample API Scalar UI",
             Version = "v1",
             Description = "API for ScalarSample.",
             Contact = new OpenApiContact
             {
                 Url = new Uri("http://www.muslumozturk.com"),
                 Email = "muslum.ozturk@hotmail.com",
                 Name = "Müslüm ÖZTÜRK"
             }
         };
         return Task.CompletedTask;
     });
 });

```


``` csharp
 app.MapScalarApiReference(options =>
 {
     options.Title = "ScalarSample API Scalar UI";
     options.ShowSidebar = true;
     options.Theme = ScalarTheme.BluePlanet;

     options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);

     options.Servers = new List<ScalarServer>()
                        {
                            new ScalarServer("https://localhost:7100"),
                            new ScalarServer("http://localhost:5100"),
                        };


 });
```

in launchSettings.json file
``` json
 "launchBrowser": true,
 "launchUrl": "scalar/v1",
 "applicationUrl": "http://localhost:5100"
```

``` json
"launchBrowser": true,
"launchUrl": "scalar/v1",
"applicationUrl": "https://localhost:7100;http://localhost:5100"
```

action attributes for documentation
``` csharp
  [HttpGet("GetWeatherForecast")]
  [EndpointSummary("This is a summary from OpenApi attributes.")]
  [EndpointDescription("This is a description from OpenApi attributes.")]
  [EndpointName("FromAttributes")]
  [Produces(typeof(IEnumerable<WeatherForecast>))]
```

model documentation with attributes
```csharp
 public class WeatherForecast
 {
     [Description("This is a Date.")]
     public DateOnly Date { get; set; }

     public int TemperatureC { get; set; }

     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

     [Description("This is a Summary.")]
     public string? Summary { get; set; }
 }
```


execute this command to run keycloak on docker

``` sh
docker run --name keycloak-sample -p 8080:8080 -e KEYCLOAK_ADMIN=mozturk -e KEYCLOACK_ADMIN_PASSWORD=P@ssword1! -e KC_BOOTSTRAP_ADMIN_USERNAME=mozturk -e KC_BOOTSTRAP_ADMIN_PASSWORD=P@ssword1!  quay.io/keycloak/keycloak start-dev
```

add Microsoft.AspNetCore.Authentication.JwtBearer reference from nuget

```
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.0
```

in program.cs

``` csharp
 builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer(o =>
     {
         o.RequireHttpsMetadata = false;
         o.Audience = "account";
         o.MetadataAddress = "http://localhost:8080/realms/sample-realm/.well-known/openid-configuration";
         o.TokenValidationParameters = new TokenValidationParameters
         {
             ValidIssuer = "http://localhost:8080/realms/sample-realm"
         };
     });

```

``` csharp
 builder.Services.AddOpenApi(options =>
 {
    //....
     options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
 });
```

add BearerSecuritySchemeTransformer class

```
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
```

add new action into WeatherForecastController.cs
```
  [Authorize]
  [HttpGet("Claims")]
  public Dictionary<string, string> GetClaims()
  {
      return User.Claims.ToDictionary(c => c.Type, c => c.Value);
  }
```

