
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace ScalarSample.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
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

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
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
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
