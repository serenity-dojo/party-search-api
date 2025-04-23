using Microsoft.OpenApi.Models;
using PartySearchApi.Api.Models;
using PartySearchApi.Api.Repositories;
using PartySearchApi.Api.Services;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Party Search API",
        Version = "v1",
        Description = "API for searching sanctioned parties in the banking system"
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Register application services
builder.Services.AddSingleton<IPartyRepository, InMemoryPartyRepository>();
builder.Services.AddScoped<IPartyService, PartyService>();
builder.Services.AddScoped<PartyDataSeeder>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var urls = app.Urls.ToArray();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("===========================================================");
    Console.WriteLine("Party Search API is running on the following URLs:");
    foreach (var url in urls)
    {
        Console.WriteLine($"  {url}");
    }
    Console.ResetColor();
});


// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevelopmentPolicy");

    string? seedDataFile = GetSeedDataFileArgument(Environment.GetCommandLineArgs());

    if (!string.IsNullOrEmpty(seedDataFile))
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<PartyDataSeeder>();
        await seeder.SeedFromJsonFile(seedDataFile);
    }
}
else
{
    // In production, use more restrictive CORS and enable HSTS
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Helper method to get the seed data file path from command line arguments
static string? GetSeedDataFileArgument(string[] args)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], "--seed-data", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(args[i], "--seeddata", StringComparison.OrdinalIgnoreCase))
        {
            return args[i + 1];
        }
    }
    return null;
}

public partial class Program { }