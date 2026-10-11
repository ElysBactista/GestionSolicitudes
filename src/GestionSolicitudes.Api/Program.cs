using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestionSolicitudes.Application.Interfaces;
using GestionSolicitudes.Infrastructure.Persistence;
using GestionSolicitudes.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Base de Datos (Entity Framework Core con SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Inyección de Dependencias de Servicios de Negocio
builder.Services.AddScoped<ISolicitudService, SolicitudService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// 4. CORS: Permite peticiones desde el cliente Blazor
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 5. Configuración de ASP.NET Core Identity (Reglas de contraseña)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 6. Configuración de Autenticación con Token JWT (Soporte sin 403 Forbidden)
const string jwtKeyFija = "ClaveSuperSecretaYLargaParaFirmarLosTokensJWT2026!";
var jwtKey = builder.Configuration["Jwt:Key"] ?? jwtKeyFija;

// Limpia el mapeo arbitrario para que los claims no se muten internamente
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(10),
        RoleClaimType = "role",
        NameClaimType = "name"
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
            {
                // Asegurar que tanto 'role' como ClaimTypes.Role existan en la identidad
                var roles = identity.FindAll("role").Select(c => c.Value)
                            .Concat(identity.FindAll(ClaimTypes.Role).Select(c => c.Value))
                            .Distinct()
                            .ToList();

                foreach (var rol in roles)
                {
                    if (!identity.HasClaim(ClaimTypes.Role, rol))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, rol));
                    }
                    if (!identity.HasClaim("role", rol))
                    {
                        identity.AddClaim(new Claim("role", rol));
                    }
                }
            }
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// 7. Pipeline de peticiones HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Habilitar servicio de archivos estáticos para descargas y visor de recaudos (I4)
app.UseStaticFiles();

// CORS debe ir ANTES de Authentication y Authorization
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 8. Aplicar migraciones y asegurar roles al arrancar
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    await context.Database.MigrateAsync();
    await DbInitializer.SeedAsync(roleManager);
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR INICIO BD]: {ex.Message}");
}

app.Run();