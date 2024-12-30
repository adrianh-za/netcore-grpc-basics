using System.Security.Claims;
using Grpc.FirstServer.Lib;
using Grpc.FirstServer.Services;
using Grpc.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(opt =>
{
    opt.Interceptors.Add<ServerLoggerInterceptor>();
    opt.ResponseCompressionAlgorithm = "gzip";
    opt.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
});

//Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = false,
            ValidateActor = false,
            IssuerSigningKey = JwtHelper.SecurityKey
        };
    });

//Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, polOptions => { polOptions.RequireClaim(ClaimTypes.Name); });
});

//Health checks
builder.Services.AddGrpcHealthChecks()
    .AddCheck(
        "FirstService",
        () => HealthCheckResult.Healthy("The check is healthy"),
        ["FirstService", "grpc"]
    );

// Reflection
builder.Services.AddGrpcReflection();

var app = builder.Build();


// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<FirstService>();
app.MapGrpcHealthChecksService();   //NB: This is the health check service
app.MapGrpcReflectionService();     //NB: This is the reflection service to expose the gRPC services

app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

//NOTE: Not needed as we are using gRPC
//app.UseAuthentication();
//app.UseAuthorization();

app.Run();

//SO it can be referenced in other projects (Seems a crap way to be honest!!!)
public partial class Program
{
}