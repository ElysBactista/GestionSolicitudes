using System.IO;
using System.Threading.Tasks;
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
    // GET: api/solicitud/mi-expediente
    [HttpGet("mi-expediente")]
    public async Task<ActionResult<ExpedienteResidenteResponseDto>> ObtenerMiExpediente([FromQuery] string? email)
    {
        var query = context.Solicitudes
            .Include(s => s.Documentos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(email))
        {
            query = query.Where(s => s.Solicitante.Contains(email) || s.Titulo.Contains(email));
        }

        var solicitud = await query.OrderByDescending(s => s.FechaCreacion).FirstOrDefaultAsync();

        if (solicitud == null)
        {
            return NotFound("No se encontró ningún expediente activo.");
        }

        int paso = solicitud.Estado switch
        {
            EstadoSolicitud.Pendiente => 1, // Radicación
            EstadoSolicitud.EnProceso => 2, // Evaluación Legal
            EstadoSolicitud.Aprobada => 3,  // Resolución
            _ => 1
        };

        var respuesta = new ExpedienteResidenteResponseDto
        {
            Id = solicitud.Id,
            NumeroRadicado = $"EXP-{solicitud.FechaCreacion.Year}-{solicitud.Id:D4}",
            TipoResidencia = string.IsNullOrWhiteSpace(solicitud.Titulo) ? "Residencia Temporal por Trabajo (RT-3)" : solicitud.Titulo,
            NombreTitular = solicitud.Solicitante,
            NumeroPasaporte = "P0948214",
            PaisOrigen = "Colombia",
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
                Estado = d.Estado,
                UrlDescarga = $"uploads/{Path.GetFileName(d.RutaAlmacenamiento)}"
            }).ToList()
        };

        return Ok(respuesta);
    }

    // POST: api/solicitud/subir-documento
    [HttpPost("subir-documento")]
    public async Task<IActionResult> SubirDocumentoAnexo([FromForm] int solicitudId, [FromForm] string tipoDocumento, IFormFile archivo)
    {
        var solicitud = await context.Solicitudes.FindAsync(solicitudId);
        if (solicitud == null)
            return NotFound("El expediente no existe.");

        if (archivo == null || archivo.Length == 0)
            return BadRequest("Seleccione un archivo válido.");

        var uploadsFolder = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var extension = Path.GetExtension(archivo.FileName).ToLower();
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
            Estado = "En Revisión",
            FechaSubida = DateTime.UtcNow
        };

        context.DocumentosAdjuntos.Add(nuevoDoc);
        await context.SaveChangesAsync();

        return Ok(new { Mensaje = "Documento cargado correctamente." });
    }

    // GET: api/solicitud
    [HttpGet]
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

    // PATCH: api/solicitud/documento/{documentoId}/estado
    [HttpPatch("documento/{documentoId}/estado")]
    public async Task<IActionResult> ActualizarEstadoDocumento(int documentoId, [FromBody] ActualizarEstadoDocumentoDto dto)
    {
        var documento = await context.DocumentosAdjuntos.FindAsync(documentoId);
        if (documento == null)
            return NotFound("El documento no fue encontrado.");

        documento.Estado = dto.NuevoEstado;
        await context.SaveChangesAsync();

        return Ok(new { Mensaje = "Estado del documento actualizado correctamente." });
    }

    // GET: api/solicitud/{solicitudId}/documentos
    [HttpGet("{solicitudId}/documentos")]
    public async Task<ActionResult<List<DocumentoDto>>> ObtenerDocumentosPorSolicitud(int solicitudId)
    {
        var docs = await context.DocumentosAdjuntos
            .Where(d => d.SolicitudId == solicitudId)
            .OrderByDescending(d => d.FechaSubida)
            .Select(d => new DocumentoDto
            {
                Id = d.Id,
                Nombre = d.TipoDocumento,
                Icono = d.Extension == ".pdf" ? "bi-file-earmark-pdf text-danger" : "bi-file-earmark-image text-primary",
                FechaSubida = d.FechaSubida,
                Estado = d.Estado,
                UrlDescarga = $"uploads/{Path.GetFileName(d.RutaAlmacenamiento)}"
            })
            .ToListAsync();

        return Ok(docs);
    }

    // POST: api/solicitud/radicar
    [HttpPost("radicar")]
    public async Task<IActionResult> RadicarNuevaSolicitud([FromBody] CrearSolicitudDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo) || string.IsNullOrWhiteSpace(dto.Solicitante))
        {
            return BadRequest("El tipo de trámite y el solicitante son requeridos.");
        }

        var nuevaSolicitud = new Solicitud
        {
            Titulo = dto.Titulo,
            Solicitante = dto.Solicitante,
            Descripcion = dto.Descripcion ?? string.Empty,
            Estado = EstadoSolicitud.Pendiente,
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };

        context.Solicitudes.Add(nuevaSolicitud);
        await context.SaveChangesAsync();

        return Ok(new { Mensaje = "Solicitud radicada con éxito.", Id = nuevaSolicitud.Id });
    }

    // GET: api/solicitud/filtradas
    [HttpGet("filtradas")]
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

    // GET: api/solicitud/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var solicitud = await solicitudService.ObtenerPorIdAsync(id);
        if (solicitud == null) return NotFound("Solicitud no encontrada.");
        return Ok(solicitud);
    }

    // POST: api/solicitud
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearSolicitudDto dto)
    {
        var creada = await solicitudService.CrearAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    // PUT: api/solicitud/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarSolicitudDto dto)
    {
        var resultado = await solicitudService.ActualizarAsync(id, dto);
        if (!resultado) return NotFound("La solicitud no existe o está inactiva.");
        return Ok("Solicitud actualizada correctamente.");
    }

    // PATCH: api/solicitud/{id}/estado
    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] ActualizarEstadoDto dto)
    {
        var resultado = await solicitudService.CambiarEstadoAsync(id, dto);
        if (!resultado) return NotFound("La solicitud no existe o está inactiva.");
        return NoContent();
    }

    // DELETE: api/solicitud/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var resultado = await solicitudService.DesactivarAsync(id);
        if (!resultado) return NotFound("La solicitud no existe o está inactiva.");
        return NoContent();
    }

    // GET: api/solicitud/dashboard
    [HttpGet("dashboard")]
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