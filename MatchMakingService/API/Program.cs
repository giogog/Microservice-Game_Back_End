using API.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.GeneralConfiguration(builder.Configuration);
builder.Services.ConfigureMatchMakingBackgroundService();




builder.Host.UseSerilog();

var app = builder.Build();
Log.Information($"MatchmakingService Starting Up on port: {builder.Configuration["Host"]}"); 


app.UseSerilogRequestLogging();

app.Run();
