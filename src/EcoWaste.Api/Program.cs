using System.Text;
using EcoWaste.Api.Data;
using EcoWaste.Api.Middleware;
using EcoWaste.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ----- Banco de dados (Oracle) -----
var connectionString = builder.Configuration.GetConnectionString("OracleConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(connectionString));

// ----- JWT -----
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// ----- Serviços de aplicação -----
builder.Services.AddScoped<ICollectionPointService, CollectionPointService>();
builder.Services.AddScoped<IWasteRecordService, WasteRecordService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ----- Swagger com suporte a JWT -----
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EcoWaste API",
        Version = "v1",
        Description = "API de Gestão de Resíduos e Reciclagem (ESG) — .NET 8 / Oracle"
    });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT. Ex.: 'Bearer {seu_token}'",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, Array.Empty<string>() } });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// ----- Pipeline -----
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcoWaste API v1"));
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ----- Health check (usado pelo Docker, pelo docker-compose e pelo pipeline CI/CD) -----
// Retorna o ambiente em execução (Staging/Production) e se o banco está acessível.
app.MapGet("/health", async (AppDbContext db, IWebHostEnvironment env) =>
{
    var dbOk = await db.Database.CanConnectAsync();
    var payload = new
    {
        status = dbOk ? "Healthy" : "Unhealthy",
        environment = env.EnvironmentName,
        database = dbOk ? "up" : "down",
        version = Environment.GetEnvironmentVariable("APP_VERSION") ?? "local",
        timestampUtc = DateTime.UtcNow
    };
    return dbOk ? Results.Ok(payload) : Results.Json(payload, statusCode: StatusCodes.Status503ServiceUnavailable);
}).AllowAnonymous();

// ----- Migração + seed na inicialização -----
// Em ambiente de teste, a fábrica de testes cuida da criação/seed do banco InMemory.
// Em containers o banco pode demorar a aceitar conexões, então tentamos algumas vezes.
if (!app.Environment.IsEnvironment("Testing"))
{
    const int maxAttempts = 10;
    for (var attempt = 1; ; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbSeeder.SeedAsync(db);
            app.Logger.LogInformation("Banco migrado e populado (ambiente: {Env}).", app.Environment.EnvironmentName);
            break;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            app.Logger.LogWarning(ex, "Banco indisponível (tentativa {Attempt}/{Max}). Nova tentativa em 10s...", attempt, maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}

app.Run();

// Necessário para o WebApplicationFactory nos testes de integração.
public partial class Program { }
