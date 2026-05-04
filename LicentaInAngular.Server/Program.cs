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
    config.Description = "Documentație pentru API-ul LicentaInAngular.Server.";
    config.AddSecurity("JWT", new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "Introduceți token-ul JWT în formatul: Bearer {token}"
    });
    config.OperationProcessors.Add(new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("JWT"));
});


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AutoPaintsDb"))
           .EnableSensitiveDataLogging();  
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IPersoanaRepository, PersoanaRepository>();
builder.Services.AddScoped<ICosRepository, CosRepository>();
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<ISubprodusRepository, SubprodusRepository>();
builder.Services.AddScoped<IAdresaRepository, AdresaRepository>();
builder.Services.AddScoped<IComandaRepository, ComandaRepository>();
builder.Services.AddScoped<ISubcomandaRepository, SubcomandaRepository>();
builder.Services.AddScoped<IPersoanaRepository, PersoanaRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddSingleton<IPredictionService, PredictionService>();
builder.Services.AddScoped<IGPTService, AimlApiService>();


builder.Services.AddHttpClient("PaintAnalysisClient", client =>
{

    client.BaseAddress = new Uri("http://127.0.0.1:5001/");
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()    // Permite orice origine (ideal pentru dezvoltare)
              .AllowAnyMethod()    // Permite toate metodele HTTP
              .AllowAnyHeader();   // Permite orice header
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


var app = builder.Build();

// Utilizare CORS
app.UseCors("AllowAll");

// Middleware pentru gestionarea erorilor
app.UseMiddleware<ErrorHandlerMiddleware>();

// Configurarea NSwag
app.UseHttpsRedirection();
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();


app.MapControllers();

app.Run();
