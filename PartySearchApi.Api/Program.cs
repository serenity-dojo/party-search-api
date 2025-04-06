using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using PartySearchApi.Api.Repositories;
using PartySearchApi.Api.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Party Search API",
        Version = "v1",
        Description = "API for searching sanctioned parties"
    });

    c.SchemaFilter<EnumSchemaFilter>();
});

builder.Services.AddSingleton<IPartyRepository, InMemoryPartyRepository>();
builder.Services.AddScoped<IPartySearchService, PartySearchService>();

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            schema.Type = "string";

            foreach (var name in Enum.GetNames(context.Type))
            {
                schema.Enum.Add(new OpenApiString(name));
            }
        }
    }
}


// Make the Program class public so it can be used by the tests
public partial class Program { }