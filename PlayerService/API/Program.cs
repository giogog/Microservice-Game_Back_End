using API.Extensions;
using Infrastructure.Context;
using Infrastructure.GrpcClient;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.ConfigureCors();

//configure dbs
if (builder.Environment.IsProduction())
{
    builder.Services.ConfigureSqlServer(builder.Configuration, "PlayerConn");
}
else
{
    builder.Services.ConfigureSqlServer(builder.Configuration, "DefaultConnection");
}

builder.Services.ConfigureIdentityService(builder.Configuration);

builder.Services.ConfigureRepositoryManager();

builder.Services.GeneralConfiguration(builder.Configuration);
builder.Services.ConfigureAutomapper();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGrpc();
builder.Services.AddSwaggerGen();
builder.ConfigureSwagger();




builder.Services.AddSingleton<IDataProtectionProvider, EphemeralDataProtectionProvider>();


builder.Host.UseSerilog();

var app = builder.Build();
Log.Information($"PlayerService Starting Up on url: {builder.Configuration["Host"]}"); 



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate(); // Applies migrations automatically
            Log.Information("Migrated succesfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An error occurred while migrating the database.");
            throw; // Optionally rethrow or handle as needed
        }
    }
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();




app.UseSerilogRequestLogging();
app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();


app.MapGrpcService<Players>();
app.MapControllers();

app.Run();
