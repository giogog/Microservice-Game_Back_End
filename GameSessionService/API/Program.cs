using GameSession.API.Extensions;
using GameSession.Application.SignalR;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

// Custom configurations
builder.Services.ConfigureCors();
builder.Services.AddSignalR();
//configure dbs
builder.Services.ConfigureMongoDb(builder.Configuration);
builder.Services.GeneralConfiguration(builder.Configuration);

builder.Services.ConfigureAutomapper();


builder.Services.ConfigureRepositoryManager();

// JWT Configuration
builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.AddAuthorization();


builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureMediatR();
builder.Services.ConfigureGameSessionBackgroundService();
builder.Services.ConfigureGameSessionmMonitoringBackgroundService(); 
builder.Services.AddSwaggerGen();
builder.Services.AddGrpc();
builder.ConfigureSwagger();



builder.Host.UseSerilog();

var app = builder.Build();
Log.Information($"GamesessionService Starting Up on port: {builder.Configuration["Host"]}"); 



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();


app.UseSerilogRequestLogging();
app.UseRouting();
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();



app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<GameSessionHub>("/GameSessionHub").RequireAuthorization();
    endpoints.MapControllers();
});


app.Run();

