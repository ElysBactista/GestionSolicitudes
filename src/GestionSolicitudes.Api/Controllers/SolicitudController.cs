using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSolicitudes.Application.DTOs;
using GestionSolicitudes.Application.Interfaces;
using GestionSolicitudes.Domain;
using GestionSolicitudes.Infrastructure.Persistence;

namespace GestionSolicitudes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolicitudController(
    ISolicitudService solicitudService,
    ApplicationDbContext context,
    IWebHostEnvironment env) : ControllerBase
{
    private static readonly string[] ExtensionesPermitidas = [".pdf", ".jpg", ".jpeg", ".png"];
    private const long MaxTamanoArchivoBytes = 5 * 1024 * 1024; // 5 MB (RQ6)

    // GET: api/solicitud/mias (Mis Solicitudes del Residente)
    [HttpGet("mias")]
    [Authorize]
    public async Task<ActionResult<List<MiSolicitudResumenDto>>> ObtenerMisSolicitudes()
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            return Unauthorized("Usuario no identificado en la sesión actual.");
        }

        var misSolicitudes = await solicitudService.ObtenerMiasAsync(usuarioId);
        return Ok(misSolicitudes);
    }

    // GET: api/solicitud/mi-expediente
    [HttpGet("mi-expediente")]
    [Authorize]
    public async Task<ActionResult<ExpedienteResidenteResponseDto>> ObtenerMiExpediente([FromQuery] string? email)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var esAdmin = User.IsInRole("Administrador");

        var query = context.Solicitudes
            .Include(s => s.Documentos)
            .AsQueryable();

        if (!esAdmin && !string.IsNullOrWhiteSpace(usuarioId))
        {
            query = query.Where(s => s.UsuarioId == usuarioId);
        }
        else if (!string.IsNullOrWhiteSpace(email))
        {
            query = query.Where(s => s.Solicitante.Contains(email) || s.Titulo.Contains(email) || s.Correo.Contains(email));
        }

        var solicitud = await query.OrderByDescending(s => s.FechaCreacion).FirstOrDefaultAsync();

        if (solicitud == null)
        {
            return NotFound("No se encontró ningún expediente activo.");
        }

        int paso = solicitud.Estado switch
        {
            EstadoSolicitud.Pendiente => 1,
            EstadoSolicitud.EnProceso => 2,
            EstadoSolicitud.Aprobada => 3,
            _ => 1
        };

        var respuesta = new ExpedienteResidenteResponseDto
        {
            Id = solicitud.Id,
            NumeroRadicado = string.IsNullOrWhiteSpace(solicitud.Codigo)
                ? $"EXP-{solicitud.FechaCreacion.Year}-{solicitud.Id:D4}"
                : solicitud.Codigo,
            TipoResidencia = string.IsNullOrWhiteSpace(solicitud.Titulo) ? "Residencia Temporal por Trabajo (RT-3)" : solicitud.Titulo,
            NombreTitular = solicitud.Solicitante,
            NumeroPasaporte = string.IsNullOrWhiteSpace(solicitud.NumeroPasaporte) ? "P0948214" : solicitud.NumeroPasaporte,
            PaisOrigen = string.IsNullOrWhiteSpace(solicitud.Nacionalidad) ? "Colombia" : solicitud.Nacionalidad,
            PasoActual = paso,
            Estado = (int)solicitud.Estado,
            FechaEmision = solicitud.FechaCreacion,
            FechaVencimiento = solicitud.FechaCreacion.AddYears(1),
            Documentos = solicitud.Documentos.Select(d => new DocumentoDto
            {
                Id = d.Id,
                Nombre = d.TipoDocumento,
                Icono = d.Extension switch
                {
                    ".pdf" => "bi-file-earmark-pdf text-danger",
                    ".jpg" or ".jpeg" or ".png" => "bi-file-earmark-image text-primary",
                    _ => "bi-file-earmark text-secondary"
                },
                FechaSubida = d.FechaSubida,
                Estado = d.Estado.ToString(),
                UrlDescarga = $"{Request.Scheme}://{Request.Host}/uploads/{Path.GetFileName(d.RutaAlmacenamiento)}"
            }).ToList()
        };

        return Ok(respuesta);
    }

    // POST: api/solicitud/radicar
    [HttpPost("radicar")]
    [Authorize]
    public async Task<IActionResult> RadicarNuevaSolicitud([FromBody] CrearSolicitudDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo) || (string.IsNullOrWhiteSpace(dto.Solicitante) && string.IsNullOrWhiteSpace(dto.Nombres)))
        {
            return BadRequest("El tipo de trámite y el solicitante son requeridos.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("Debe autenticarse para radicar una solicitud.");
        }

        dto.UsuarioId = userId;

        var creada = await solicitudService.CrearAsync(dto);
        return Ok(new { Mensaje = "Solicitud radicada con éxito.", Id = creada.Id });
    }

    // POST: api/solicitud/subir-documento
    [HttpPost("subir-documento")]
    [Authorize]
    public async Task<IActionResult> SubirDocumentoAnexo([FromForm] int solicitudId, [FromForm] string tipoDocumento, IFormFile archivo)
    {
        var solicitud = await context.Solicitudes.FindAsync(solicitudId);
        if (solicitud == null)
            return NotFound("El expediente no existe.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var esAdmin = User.IsInRole("Administrador");

        // Regla de propiedad: Solo el dueño de la solicitud o un Administrador pueden subir recaudos
        if (!esAdmin && solicitud.UsuarioId != userId)
        {
            return Forbid();
        }

        if (archivo == null || archivo.Length == 0)
            return BadRequest("Seleccione un archivo válido.");

        if (archivo.Length > MaxTamanoArchivoBytes)
            return BadRequest("El tamaño del archivo no puede exceder los 5 MB.");

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(extension))
            return BadRequest("Formato de archivo no permitido. Solo se aceptan extensiones .pdf, .jpg, .jpeg o .png.");

        var uploadsFolder = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var nombreUnico = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(uploadsFolder, nombreUnico);

        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
        {
            await archivo.CopyToAsync(stream);
        }

        var nuevoDoc = new DocumentoAdjunto
        {
            SolicitudId = solicitudId,
            TipoDocumento = tipoDocumento,
            NombreArchivoOriginal = archivo.FileName,
            RutaAlmacenamiento = rutaCompleta,
            Extension = extension,
            TamanoBytes = archivo.Length,
            Estado = EstadoDocumento.EnRevision,
            FechaSubida = DateTime.UtcNow
        };

        context.DocumentosAdjuntos.Add(nuevoDoc);
        await context.SaveChangesAsync();

        return Ok(new { Mensaje = "Documento cargado correctamente." });
    }

    // GET: api/solicitud/{solicitudId}/documentos
    [HttpGet("{solicitudId}/documentos")]
    [Authorize]
    public async Task<ActionResult<List<DocumentoDto>>> ObtenerDocumentosPorSolicitud(int solicitudId)
    {
        var solicitud = await context.Solicitudes.FindAsync(solicitudId);
        if (solicitud == null)
            return NotFound("Expediente no encontrado.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var esAdmin = User.IsInRole("Administrador");

        if (!esAdmin && solicitud.UsuarioId != userId)
        {
            return Forbid();
        }

        var docs = await context.DocumentosAdjuntos
            .Where(d => d.SolicitudId == solicitudId)
            .OrderByDescending(d => d.FechaSubida)
            .Select(d => new DocumentoDto
            {
                Id = d.Id,
                Nombre = d.TipoDocumento,
                Icono = d.Extension == ".pdf" ? "bi-file-earmark-pdf text-danger" : "bi-file-earmark-image text-primary",
                FechaSubida = d.FechaSubida,
                Estado = d.Estado.ToString(),
                UrlDescarga = $"{Request.Scheme}://{Request.Host}/uploads/{Path.GetFileName(d.RutaAlmacenamiento)}"
            })
            .ToListAsync();

        return Ok(docs);
    }

  
    // GET: api/solicitud/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var entidad = await context.Solicitudes.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id && s.Activo);
        if (entidad == null) return NotFound("Solicitud no encontrada.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var esAdmin = User.IsInRole("Administrador");

        if (!esAdmin && entidad.UsuarioId != userId)
        {
            return Forbid();
        }

        var solicitud = await solicitudService.ObtenerPorIdAsync(id);
        return Ok(solicitud);
    }

    // POST: api/solicitud
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Crear([FromBody] CrearSolicitudDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("Usuario no identificado.");
        }

        dto.UsuarioId = userId;
        var creada = await solicitudService.CrearAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    // ==========================================
    // ENDPOINTS ADMINISTRATIVOS (ROLES = "Administrador")
    // ==========================================

    // GET: api/solicitud
    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ObtenerTodas(
        [FromQuery] string? busqueda,
        [FromQuery] int numeroPagina = 1,
        [FromQuery] int tamanoPagina = 10)
    {
        var (Total, Solicitudes) = await solicitudService.ObtenerTodasAsync(busqueda, numeroPagina, tamanoPagina);

        return Ok(new
        {
            TotalRegistros = Total,
            NumeroPagina = numeroPagina,
            TamanoPagina = tamanoPagina,
            Datos = Solicitudes
        });
    }

    // GET: api/solicitud/filtradas
    [HttpGet("filtradas")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<List<SolicitudItemDto>>> ObtenerFiltradas(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] string? busqueda,
        [FromQuery] string? estado,
        [FromQuery] string? tipoTramite)
    {
        try
        {
            var resultado = await solicitudService.ObtenerFiltradasAsync(fechaInicio, fechaFin, busqueda, estado, tipoTramite);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener solicitudes filtradas: {ex.Message}");
        }
    }

    // PATCH: api/solicitud/documento/{documentoId}/estado
    [HttpPatch("documento/{documentoId}/estado")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActualizarEstadoDocumento(int documentoId, [FromBody] ActualizarEstadoDocumentoDto dto)
    {
        var documento = await context.DocumentosAdjuntos.FindAsync(documentoId);
        if (documento == null)
            return NotFound("El documento no fue encontrado.");

        if (Enum.TryParse<EstadoDocumento>(dto.NuevoEstado, true, out var nuevoEstadoEnum))
        {
            documento.Estado = nuevoEstadoEnum;
        }
        else
        {
            documento.Estado = EstadoDocumento.EnRevision;
        }

        await context.SaveChangesAsync();

        return Ok(new { Mensaje = "Estado del documento actualizado correctamente." });
    }

    // PUT: api/solicitud/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarSolicitudDto dto)
    {
        var resultado = await solicitudService.ActualizarAsync(id, dto);
        if (!resultado) return NotFound("La solicitud no existe o está inactiva.");
        return Ok("Solicitud actualizada correctamente.");
    }

    // PATCH: api/solicitud/{id}/estado
    [HttpPatch("{id}/estado")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] ActualizarEstadoDto dto)
    {
        var resultado = await solicitudService.CambiarEstadoAsync(id, dto);
        if (!resultado) return NotFound("La solicitud no existe o está inactiva.");
        return NoContent();
    }

    // DELETE: api/solicitud/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var resultado = await solicitudService.DesactivarAsync(id);
        if (!resultado) return NotFound("La solicitud no existe o está inactiva.");
        return NoContent();
    }

    // GET: api/solicitud/dashboard
    [HttpGet("dashboard")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<DashboardDto>> ObtenerResumenDashboard(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] string? busqueda,
        [FromQuery] string? estado,
        [FromQuery] string? tipoTramite)
    {
        try
        {
            var resumen = await solicitudService.ObtenerResumenDashboardAsync(fechaInicio, fechaFin, busqueda, estado, tipoTramite);
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al cargar el dashboard: {ex.Message}");
        }
    }
}