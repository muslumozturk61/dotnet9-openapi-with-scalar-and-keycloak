# .Net 9 OpenApi With Scalar and Keyclaok


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

execute this command to run keycloak on docker

``` sh
docker run --name keycloak-sample -p 8080:8080 -e KEYCLOAK_ADMIN=mozturk -e KEYCLOACK_ADMIN_PASSWORD=P@ssword1! -e KC_BOOTSTRAP_ADMIN_USERNAME=mozturk -e KC_BOOTSTRAP_ADMIN_PASSWORD=P@ssword1!  quay.io/keycloak/keycloak start-dev
```
