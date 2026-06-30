//using System.Reflection;

//var builder = WebApplication.CreateBuilder(args);

//// Force Development environment for Swagger to work
//builder.Environment.EnvironmentName = "Development";

//// Add services to the container.
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// Security: Register Singleton for IdempotencyStore
//builder.Services.AddSingleton<IdempotencyStore>();

//// Performance: Use Scoped for business logic services
//builder.Services.AddScoped<AttributionService>();
//builder.Services.AddScoped<PerformanceCalculationService>();

//// Security: Add CORS policy
//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(policy =>
//    {
//        policy.AllowAnyOrigin()
//              .AllowAnyMethod()
//              .AllowAnyHeader();
//    });
//});

//// Performance: Add response caching
//builder.Services.AddResponseCaching();

//// Performance: Add compression
//builder.Services.AddResponseCompression(options =>
//{
//    options.EnableForHttps = true;
//    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
//    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
//        new[] { "application/json" }
//    );
//});

//// Security: Configure JSON serializer options
//builder.Services.Configure<System.Text.Json.JsonSerializerOptions>(options =>
//{
//    options.PropertyNameCaseInsensitive = true;
//    options.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
//    options.WriteIndented = false;
//});

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.Limits.MaxRequestBodySize = 1_048_576;
//    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(15);
//    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(10);
//});

//var app = builder.Build();

//// Enable compression
//app.UseResponseCompression();

//// SWAGGER - ALWAYS ENABLE FOR LOCAL TESTING
//app.UseSwagger();
//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Portfolio Performance API v1.0");
//    c.RoutePrefix = string.Empty;
//});

//Console.WriteLine("═══════════════════════════════════════════════════════════");
//Console.WriteLine("🟢 Swagger UI is ENABLED");
//Console.WriteLine("📍 Access at: https://localhost:7001");
//Console.WriteLine("═══════════════════════════════════════════════════════════");

//app.UseHttpsRedirection();
//app.UseCors();
//app.UseAuthorization();

//app.MapControllers();

//app.Run();


using System.Reflection;
using System.IO;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<AttributionService>();
builder.Services.AddScoped<IdempotencyStore>();
builder.Services.AddScoped<PerformanceCalculationService>();

// Register Swagger generation and a v1 document
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Portfolio Performance API", Version = "v1" });

    // Include XML comments if available (enable XML documentation file in project settings to use)
    try
    {
        var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    }
    catch
    {
        // ignore if assembly info not available in some hosting scenarios
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Always enable Swagger UI for local testing (root path)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Portfolio Performance API v1.0");
    c.RoutePrefix = string.Empty;
});

// NOTE: Disable HTTPS redirection in local dev to avoid issues with missing dev certificates
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Helpful runtime info for developers
Console.WriteLine("═══════════════════════════════════════════════════════════");
Console.WriteLine("🟢 Swagger UI should be available at the application root (/)");
Console.WriteLine($"📍 Listening URLs: {string.Join(", ", app.Urls)}");
Console.WriteLine("═══════════════════════════════════════════════════════════");

app.Run();
