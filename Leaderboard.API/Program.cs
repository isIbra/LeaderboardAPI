using Microsoft.Extensions.Options;
using Leaderboard.API.Infrastructure.DbContext;
using Leaderboard.API.Infrastructure.Exceptions.Filters;
using Leaderboard.API.Infrastructure.Extensions;
using Leaderboard.API.Infrastructure.Repositories;
using Leaderboard.API.Infrastructure.Services;
using Leaderboard.API.Infrastructure.Util;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.ConfigureCustomWebHost(Directory.GetCurrentDirectory())
    .ConfigureLogger()
    .ConfigureLocalization()
    .ConfigureApiVersioning(1, 0)
    .ConfigureAuthentication();

builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(HttpExceptionFilter));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AnyOrigin", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Utils
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerConfigureOptions>();


// Use environment variable for MongoDB connection if available
var mongoConnectionString = Environment.GetEnvironmentVariable("MONGODB_URI") ?? builder.Configuration["ConnectionString"];
builder.Configuration["ConnectionString"] = mongoConnectionString;
builder.Configuration["ConfigSettings:ConnectionString"] = mongoConnectionString;

Console.WriteLine($"Using MongoDB connection string: {mongoConnectionString}");

builder.Services.AddHealthChecks()
                .AddMongoDb(mongoConnectionString, tags: new[] { "service" });

// Configuration
builder.Services.Configure<ConfigSettings>(builder.Configuration.GetSection("ConfigSettings"));

//services
builder.Services.AddTransient<ILeaderboardService, LeaderboardService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IRetentionService, RetentionService>();

//repos
builder.Services.AddTransient<ILeaderboardRepository, LeaderboardRepository>();
builder.Services.AddTransient<IRetentionRepository, RetentionRepository>();

// db
builder.Services.AddSingleton<LeaderboardDbContext>();

var app = builder.Build();

try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<LeaderboardDbContext>();
        dbContext.CreateIndexes();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Failed to create MongoDB indexes: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseRouting();

// Use Swagger
app.ConfigureSwaggerApiVersioning();


app.UseCors("AnyOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapGet("/api/health", () => "Healthy");
});

// Configure health checks
app.ConfigureHealthChecks();

app.Run();
