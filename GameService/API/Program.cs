using Game.API.Extensions;
using Game.Infrastructure.GrpcServer;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Custom configurations (ensure these are correctly set up)
builder.Services.ConfigureCors();
builder.Services.ConfigureMongoDb(builder.Configuration);
builder.Services.GeneralConfiguration(builder.Configuration);
builder.Services.ConfigureRepositoryManager();

// JWT Configuration
builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// Additional services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpc();

// Swagger Configuration
builder.ConfigureSwagger();

// Build the app
var app = builder.Build();

// Log startup information
Log.Information($"GameDomainService Starting Up on port: {builder.Configuration["Host"]}");

// Development-specific middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Serilog request logging
app.UseSerilogRequestLogging();


// Enable routing
app.UseRouting();

// Enable CORS
app.UseCors("AllowSpecificOrigin");


// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();


app.MapGrpcService<Regions>();
app.MapGrpcService<Gamemodes>();
app.MapControllers();

app.Run();
