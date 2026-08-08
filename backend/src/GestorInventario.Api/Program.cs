using GestorInventario.Api.Authentication;
using GestorInventario.Api.Errors;
using GestorInventario.Api.Swagger;
using GestorInventario.Application;
using GestorInventario.Infrastructure;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
    options.AddPolicy(
        FrontendCorsPolicy,
        policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));
builder.Services.AddApiProblemDetails();
builder.Services.AddApiAuthentication();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.OperationFilter<ProblemDetailsContentTypeOperationFilter>());
builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration,
    persistDataProtectionKeys: builder.Environment.IsProduction());

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public partial class Program;
