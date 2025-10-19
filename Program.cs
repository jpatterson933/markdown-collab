using Microsoft.EntityFrameworkCore;
using MarkdownCollab.Common;
using MarkdownCollab.Data;
using MarkdownCollab.Services;
using MarkdownCollab.Hubs;
using MarkdownCollab.Middleware;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder);
ConfigureDatabase(builder);

var app = builder.Build();

EnsureDatabaseIsCreated(app);
ConfigureMiddleware(app);
ConfigureEndpoints(app);

app.Run();

static void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddRazorPages();
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    });
    builder.Services.AddSignalR();
    ConfigureSessionSupport(builder.Services);
    ConfigureRateLimiting(builder.Services, builder.Configuration);
    builder.Services.AddScoped<RoomService>();
}

static void ConfigureRateLimiting(IServiceCollection services, IConfiguration configuration)
{
    services.AddMemoryCache();
    services.Configure<IpRateLimitOptions>(options =>
    {
        options.EnableEndpointRateLimiting = true;
        options.StackBlockedRequests = false;
        options.RealIpHeader = "X-Real-IP";
        options.ClientIdHeader = "X-ClientId";
        options.GeneralRules = CreateRateLimitRules();
    });
    services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    services.AddInMemoryRateLimiting();
}

static List<RateLimitRule> CreateRateLimitRules()
{
    return new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "POST:/login",
            Period = "1m",
            Limit = 5
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/rooms/create",
            Period = "1m",
            Limit = 10
        }
    };
}

static void ConfigureSessionSupport(IServiceCollection services)
{
    services.AddDistributedMemoryCache();
    services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromHours(ApplicationConstants.Authentication.SessionIdleTimeoutHours);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });
}

static void ConfigureDatabase(WebApplicationBuilder builder)
{
    var connectionString = GetConnectionString(builder.Configuration);

    if (HasValidConnectionString(connectionString))
    {
        ConfigurePostgresDatabase(builder.Services, connectionString!);
    }
    else
    {
        ConfigureInMemoryDatabase(builder.Services);
    }
}

static string? GetConnectionString(IConfiguration configuration)
{
    return configuration.GetConnectionString(ApplicationConstants.Database.DefaultConnectionKey)
        ?? Environment.GetEnvironmentVariable(ApplicationConstants.Database.EnvironmentVariableName);
}

static bool HasValidConnectionString(string? connectionString)
{
    return !string.IsNullOrEmpty(connectionString);
}

static void ConfigurePostgresDatabase(IServiceCollection services, string connectionString)
{
    var npgsqlConnectionString = ConvertToNpgsqlFormatIfNeeded(connectionString);
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(npgsqlConnectionString));
}

static string ConvertToNpgsqlFormatIfNeeded(string connectionString)
{
    if (!connectionString.StartsWith(ApplicationConstants.Database.PostgresUrlPrefix) &&
        !connectionString.StartsWith(ApplicationConstants.Database.PostgresqlUrlPrefix))
    {
        return connectionString;
    }

    var uri = new Uri(connectionString);
    var credentials = ExtractCredentials(uri);
    var databaseName = ExtractDatabaseName(uri);

    return BuildNpgsqlConnectionString(uri, credentials.Username, credentials.Password, databaseName);
}

static (string Username, string Password) ExtractCredentials(Uri uri)
{
    var userInfo = uri.UserInfo.Split(':');
    return (userInfo[0], userInfo[1]);
}

static string ExtractDatabaseName(Uri uri)
{
    return uri.AbsolutePath.Trim('/');
}

static string BuildNpgsqlConnectionString(Uri uri, string username, string password, string database)
{
    return $"Host={uri.Host};Port={uri.Port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}

static void ConfigureInMemoryDatabase(IServiceCollection services)
{
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase(ApplicationConstants.Database.InMemoryDatabaseName));
}

static void EnsureDatabaseIsCreated(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    database.Database.EnsureCreated();
}

static void ConfigureMiddleware(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseIpRateLimiting();
    app.UseSession();
    app.UsePasswordProtection();
    app.UseAuthorization();
}

static void ConfigureEndpoints(WebApplication app)
{
    app.MapStaticAssets();
    app.MapControllers();
    app.MapRazorPages().WithStaticAssets();
    app.MapHub<DiagramHub>(ApplicationConstants.SignalR.HubPath);
}
