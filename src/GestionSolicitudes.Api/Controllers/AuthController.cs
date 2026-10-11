using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestionSolicitudes.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace GestionSolicitudes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration) : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly IConfiguration _configuration = configuration;

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var usuario = await _userManager.FindByEmailAsync(dto.Email);
        if (usuario == null)
        {
            return Unauthorized(new RespuestaAuthDto
            {
                Exito = false,
                Mensaje = "Correo o contraseña incorrectos."
            });
        }

        var passwordValido = await _userManager.CheckPasswordAsync(usuario, dto.Password);
        if (!passwordValido)
        {
            return Unauthorized(new RespuestaAuthDto
            {
                Exito = false,
                Mensaje = "Correo o contraseña incorrectos."
            });
        }

        var roles = await _userManager.GetRolesAsync(usuario);
        var rolPrincipal = roles.FirstOrDefault() ?? "Residente";
        var token = GenerarTokenJwt(usuario, rolPrincipal);

        return Ok(new RespuestaAuthDto
        {
            Exito = true,
            Token = token,
            Rol = rolPrincipal,
            NombreCompleto = usuario.UserName ?? usuario.Email ?? string.Empty,
            Correo = usuario.Email ?? string.Empty,
            Mensaje = "Inicio de sesión exitoso."
        });
    }

    // POST: api/auth/registro
    [HttpPost("registro")]
    public async Task<IActionResult> Registrar([FromBody] RegistroDto dto)
    {
        var usuarioExiste = await _userManager.FindByEmailAsync(dto.Email);
        if (usuarioExiste != null)
        {
            return BadRequest(new RespuestaAuthDto
            {
                Exito = false,
                Mensaje = "El correo electrónico ya se encuentra registrado."
            });
        }

        var nuevoUsuario = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var resultado = await _userManager.CreateAsync(nuevoUsuario, dto.Password);
        if (!resultado.Succeeded)
        {
            var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
            return BadRequest(new RespuestaAuthDto
            {
                Exito = false,
                Mensaje = errores
            });
        }

        // El registro público SIEMPRE asigna el rol Residente (Decisión de diseño K2)
        const string rolAsignado = "Residente";

        if (!await _roleManager.RoleExistsAsync(rolAsignado))
        {
            await _roleManager.CreateAsync(new IdentityRole(rolAsignado));
        }
        await _userManager.AddToRoleAsync(nuevoUsuario, rolAsignado);

        var token = GenerarTokenJwt(nuevoUsuario, rolAsignado);

        return Ok(new RespuestaAuthDto
        {
            Exito = true,
            Token = token,
            Rol = rolAsignado,
            NombreCompleto = nuevoUsuario.Email ?? string.Empty,
            Correo = nuevoUsuario.Email ?? string.Empty,
            Mensaje = "Cuenta de residente creada exitosamente."
        });
    }

    private string GenerarTokenJwt(IdentityUser usuario, string rol)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "ClaveSuperSecretaYLargaParaFirmarLosTokensJWT2026!";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "GestionSolicitudesApi";
        var jwtAudience = _configuration["Jwt:Audience"] ?? jwtIssuer;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id),
            new(ClaimTypes.Email, usuario.Email ?? string.Empty),
            new(ClaimTypes.Name, usuario.UserName ?? string.Empty),
            new(ClaimTypes.Role, rol),
            new("role", rol) // Claim redundante para garantizar compatibilidad con Blazor (K8)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}