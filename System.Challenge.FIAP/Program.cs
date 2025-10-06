using System.Challenge.FIAP.Client.Pages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Challenge.FIAP.Components;
using System.Challenge.FIAP.Services;
using System.Challenge.FIAP.Interface;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Challenge.FIAP.Configuration;
using System.Challenge.FIAP.Data;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("Iniciando FIAP Challenge API");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Connection String não configurada!");

builder.Services.AddPooledDbContextFactory<ContextDB>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        
        sqlOptions.CommandTimeout(90);
        sqlOptions.MigrationsAssembly("System.Challenge.FIAP");
    });
    
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

try
{
    Log.Information("Testando conexão com banco de dados...");
    
    using var scope = builder.Services.BuildServiceProvider().CreateScope();
    var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ContextDB>>();
    
    await using var context = await contextFactory.CreateDbContextAsync();
    
    var canConnect = await context.Database.CanConnectAsync();
    
    if (canConnect)
    {
        var connection = context.Database.GetDbConnection();           
        await connection.CloseAsync();      
        Log.Information("Conexão com banco de dados estabelecida com sucesso!");
    }
}
catch (Exception ex)
{
    Log.Error(ex, "Erro ao testar conexão com banco de dados");
    
}

builder.Services.Configure<JwtSettings>(options =>
{
    builder.Configuration.GetSection(JwtSettings.SectionName).Bind(options);
    
    var envSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
    if (!string.IsNullOrWhiteSpace(envSecretKey))
        options.SecretKey = envSecretKey;

    var envIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
    if (!string.IsNullOrWhiteSpace(envIssuer))
        options.Issuer = envIssuer;

    var envAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
    if (!string.IsNullOrWhiteSpace(envAudience))
        options.Audience = envAudience;

    var envExpiration = Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES");
    if (!string.IsNullOrWhiteSpace(envExpiration) && int.TryParse(envExpiration, out var minutes))
        options.ExpirationInMinutes = minutes;

    options.Validate();
    
    Log.Information("JWT Settings carregados e validados");
});

var jwtSettings = new JwtSettings();
builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);

jwtSettings.SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? jwtSettings.SecretKey;
jwtSettings.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? jwtSettings.Issuer;
jwtSettings.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? jwtSettings.Audience;

jwtSettings.Validate();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Warning("Autenticação falhou: {INFO}", context.Exception.Message);
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Append("Token-Expirado", "true");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Log.Information("Token validado: {User}", context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

});

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IClassCodeService, ClassCodeService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FIAP Challenge API",
        Version = "v1",
        Description = "Desafio Processo Seletivo FIAP Challenge",
        Contact = new OpenApiContact
        {
            Name = "FIAP Team",
            Email = "contato@fiap.com.br"
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT: Bearer {seu-token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/Fiap/{documentName}/swagger.json";
    });
    
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/Fiap/v1/swagger.json", "FIAP Challenge API v1");
        c.RoutePrefix = "swagger/Fiap";
        c.DocumentTitle = "FIAP Challenge API";
        c.DefaultModelsExpandDepth(-1);
    });
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Log.Information("FIAP Challenge API iniciada em http://localhost:5000");

var maxRetries = 3;
var retryCount = 0;

while (retryCount < maxRetries)
{
    try
    {
        Log.Information("Tentativa {RetryCount}/{MaxRetries} de iniciar a aplicação", retryCount + 1, maxRetries);

app.Run();
        
        break;
    }
    catch (Exception ex)
    {
        retryCount++;
        
        if (retryCount >= maxRetries)
        {
            Log.Fatal(ex, "Aplicação falhou após {MaxRetries} tentativas. Encerrando...", maxRetries);
            throw;
        }
        
        Log.Error(ex, "Erro ao executar aplicação. Tentando reiniciar em 5 segundos... (Tentativa {RetryCount}/{MaxRetries})", retryCount, maxRetries);
        
        Thread.Sleep(5000);
    }
}

Log.Information("Aplicação encerrada");
Log.CloseAndFlush();
