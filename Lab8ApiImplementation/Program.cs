using FluentValidation;
using FluentValidation.AspNetCore;
using Lab8ApiImplementation.Middlewares;
using Lab8ApiImplementation.Services;
using Lab8ApiImplementation.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// FluentValidation: validate request DTOs automatically before the action runs
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ChatRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registers GroqChatService and gives it an HttpClient from IHttpClientFactory
builder.Services.AddHttpClient<IChatService, GroqChatService>();

// CORS: allow the React app (a different origin) to call this API from the browser
// Both addresses are needed: the browser treats localhost and 127.0.0.1 as different origins
string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ---------- Middleware pipeline (order matters!) ----------
// 1. CORS first, so even error responses (500) carry the CORS headers the browser needs
app.UseCors("ReactApp");

// 2. Error handling, so it can catch errors from everything below it
app.UseMiddleware<ErrorHandlingMiddleware>();

// 3. Request logging - times the rest of the pipeline
app.UseMiddleware<RequestLoggingMiddleware>();

// Swagger UI at /swagger
app.UseSwagger();
app.UseSwaggerUI();

// 4. Endpoint execution (FluentValidation runs here, before the action)
app.MapControllers();

app.Run();
