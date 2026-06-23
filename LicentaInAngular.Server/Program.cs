using LicentaInAngular.Server.Services;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LicentaInAngular.Server.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Logging.AddConsole();
builder.Services.AddLogging();

builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "LicentaInAngular.Server API";
    config.Version = "v1";
    config.Description = "Documentatie pentru API-ul LicentaInAngular.Server.";

    config.AddSecurity("JWT", new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "Introduceti token-ul JWT in formatul: Bearer {token}"
    });

    config.OperationProcessors.Add(
        new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("JWT")
    );
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AutoPaintsDb"))
           .EnableSensitiveDataLogging();
});

// ================================
// Repository interfaces -> Services
// ================================

builder.Services.AddScoped<IUserRepository, UserService>();
builder.Services.AddScoped<IProductRepository, ProductService>();
builder.Services.AddScoped<IPersoanaRepository, PersoanaService>();
builder.Services.AddScoped<ICosRepository, CosService>();
builder.Services.AddScoped<ICardRepository, CardService>();
builder.Services.AddScoped<ISubprodusRepository, SubprodusService>();
builder.Services.AddScoped<IAdresaRepository, AdresaService>();
builder.Services.AddScoped<IComandaRepository, ComandaService>();
builder.Services.AddScoped<ISubcomandaRepository, SubcomandaService>();
builder.Services.AddScoped<ICategoryRepository, CategoryService>();
builder.Services.AddScoped<IVopseaRepository, VopseaService>();

builder.Services.AddSingleton<IPredictionService, PredictionService>();

//builder.Services.AddScoped<IGPTService, AimlApiService>();
builder.Services.AddScoped<IGPTService, GeminiService>();

// ================================
// Concrete services direct injection
// Pentru controllere care injecteaza direct Service-ul concret
// ================================

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<PersoanaService>();
builder.Services.AddScoped<CosService>();
builder.Services.AddScoped<CardService>();
builder.Services.AddScoped<SubprodusService>();
builder.Services.AddScoped<AdresaService>();
builder.Services.AddScoped<ComandaService>();
builder.Services.AddScoped<SubcomandaService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<VopseaService>();
builder.Services.AddScoped<AimlApiService>();

builder.Services.AddSingleton<PredictionService>();

builder.Services.AddHttpClient("PaintAnalysisClient", client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:5001/");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };
    });

var app = builder.Build();

app.UseCors("AllowAll");

app.UseMiddleware<ErrorHandlerMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }