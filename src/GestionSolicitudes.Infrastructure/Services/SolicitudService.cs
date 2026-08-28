using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestionSolicitudes.Application.DTOs;
using GestionSolicitudes.Application.Interfaces;
using GestionSolicitudes.Domain;
using GestionSolicitudes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionSolicitudes.Infrastructure.Services;

public class SolicitudService : ISolicitudService
{
    private readonly ApplicationDbContext _context;

    public SolicitudService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(int Total, IEnumerable<SolicitudDto> Solicitudes)> ObtenerTodasAsync(string? busqueda, int numeroPagina, int tamanoPagina)
    {
        var query = _context.Solicitudes.Where(s => s.Activo).AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            query = query.Where(s => s.Titulo.Contains(busqueda) || s.Solicitante.Contains(busqueda));
        }

        var totalRegistros = await query.CountAsync();

        var solicitudesPaginadas = await query
            .Skip((numeroPagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        var listaDtos = solicitudesPaginadas.Select(MapToDto).ToList();

        return (totalRegistros, listaDtos);
    }

    public async Task<List<SolicitudItemDto>> ObtenerFiltradasAsync(
        DateTime? fechaInicio,
        DateTime? fechaFin,
        string? busqueda,
        string? estado,
        string? tipoTramite)
    {
        var query = _context.Solicitudes
            .Where(s => s.Activo)
            .AsNoTracking()
            .AsQueryable();

        if (fechaInicio.HasValue)
        {
            var inicio = fechaInicio.Value.Date;
            query = query.Where(s => s.FechaCreacion.Date >= inicio);
        }

        if (fechaFin.HasValue)
        {
            var fin = fechaFin.Value.Date;
            query = query.Where(s => s.FechaCreacion.Date <= fin);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoLimpio = estado.Trim().ToLower().Replace(" ", "");

            EstadoSolicitud? estadoFiltrar = estadoLimpio switch
            {
                "pendiente" or "pendientes" => EstadoSolicitud.Pendiente,
                "enproceso" => EstadoSolicitud.EnProceso,
                "aprobado" or "aprobada" or "aprobadas" => EstadoSolicitud.Aprobada,
                "rechazada" or "rechazadas" or "rechazado" => EstadoSolicitud.Rechazada,
                "cancelada" or "canceladas" or "cancelado" => EstadoSolicitud.Cancelada,
                _ => null
            };

            if (estadoFiltrar.HasValue)
            {
                query = query.Where(s => s.Estado == estadoFiltrar.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.Trim().ToLower();
            query = query.Where(s =>
                s.Titulo.ToLower().Contains(b) ||
                s.Solicitante.ToLower().Contains(b) ||
                ("exp-" + s.Id.ToString()).Contains(b));
        }

        if (!string.IsNullOrWhiteSpace(tipoTramite))
        {
            var t = tipoTramite.Trim().ToLower();
            query = query.Where(s => s.Titulo.ToLower().Contains(t) || s.Descripcion.ToLower().Contains(t));
        }

        var lista = await query
            .OrderByDescending(s => s.FechaCreacion)
            .ToListAsync();

        return lista.Select(s => new SolicitudItemDto
        {
            Id = s.Id,
            NumeroExpediente = $"EXP-{s.Id:D5}",
            NombreCompleto = s.Solicitante,
            NumeroPasaporte = "P" + s.Id.ToString("D7"),
            Email = s.Solicitante.ToLower().Replace(" ", ".") + "@correo.com",
            TipoTramite = s.Titulo,
            Estado = s.Estado.ToString(),
            FechaCreacion = s.FechaCreacion
        }).ToList();
    }

    public async Task<SolicitudDto?> ObtenerPorIdAsync(int id)
    {
        var solicitud = await _context.Solicitudes
            .FirstOrDefaultAsync(s => s.Id == id && s.Activo);

        return solicitud != null ? MapToDto(solicitud) : null;
    }

    public Task<SolicitudDto> CrearAsync(CrearSolicitudDto dto)
    {
        var nuevaSolicitud = new Solicitud
        {
            Titulo = dto.Titulo,
            Descripcion = dto.Descripcion,
            Solicitante = dto.Solicitante,
            Estado = EstadoSolicitud.Pendiente,
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };

        _context.Solicitudes.Add(nuevaSolicitud);
        _context.SaveChanges();

        return Task.FromResult(MapToDto(nuevaSolicitud));
    }

    public async Task<bool> ActualizarAsync(int id, ActualizarSolicitudDto dto)
    {
        var solicitud = await _context.Solicitudes
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud == null) return false;

        solicitud.Titulo = dto.Titulo;
        solicitud.Descripcion = dto.Descripcion ?? string.Empty;
        solicitud.Solicitante = dto.Solicitante;
        solicitud.FechaActualizacion = DateTime.UtcNow;

        _context.SaveChanges();
        return true;
    }

    public async Task<bool> CambiarEstadoAsync(int id, ActualizarEstadoDto dto)
    {
        var solicitud = await _context.Solicitudes
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud == null) return false;

        solicitud.Estado = dto.NuevoEstado;
        solicitud.FechaActualizacion = DateTime.UtcNow;

        _context.SaveChanges();
        return true;
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        var solicitud = await _context.Solicitudes
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud == null) return false;

        solicitud.Activo = false;
        solicitud.FechaActualizacion = DateTime.UtcNow;

        _context.SaveChanges();
        return true;
    }

    private static SolicitudDto MapToDto(Solicitud s) => new()
    {
        Id = s.Id,
        Titulo = s.Titulo,
        Descripcion = s.Descripcion,
        Solicitante = s.Solicitante,
        Estado = s.Estado,
        FechaCreacion = s.FechaCreacion
    };

    public async Task<DashboardDto> ObtenerResumenDashboardAsync(
        DateTime? fechaInicio,
        DateTime? fechaFin,
        string? busqueda,
        string? estado,
        string? tipoTramite)
    {
        var query = _context.Solicitudes
            .Where(s => s.Activo)
            .AsNoTracking()
            .AsQueryable();

        if (fechaInicio.HasValue)
        {
            var inicio = fechaInicio.Value.Date;
            query = query.Where(s => s.FechaCreacion.Date >= inicio);
        }

        if (fechaFin.HasValue)
        {
            var fin = fechaFin.Value.Date;
            query = query.Where(s => s.FechaCreacion.Date <= fin);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoLimpio = estado.Trim().ToLower().Replace(" ", "");
            EstadoSolicitud? estadoFiltrar = estadoLimpio switch
            {
                "pendiente" or "pendientes" => EstadoSolicitud.Pendiente,
                "enproceso" => EstadoSolicitud.EnProceso,
                "aprobado" or "aprobada" or "aprobadas" => EstadoSolicitud.Aprobada,
                "rechazada" or "rechazadas" or "rechazado" => EstadoSolicitud.Rechazada,
                "cancelada" or "canceladas" or "cancelado" => EstadoSolicitud.Cancelada,
                _ => null
            };

            if (estadoFiltrar.HasValue)
            {
                query = query.Where(s => s.Estado == estadoFiltrar.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.Trim().ToLower();
            query = query.Where(s => s.Titulo.ToLower().Contains(b) || s.Solicitante.ToLower().Contains(b));
        }

        if (!string.IsNullOrWhiteSpace(tipoTramite))
        {
            var t = tipoTramite.Trim().ToLower();
            query = query.Where(s => s.Titulo.ToLower().Contains(t) || s.Descripcion.ToLower().Contains(t));
        }

        var resumen = new DashboardDto
        {
            TotalSolicitudes = await query.CountAsync(),
            Pendientes = await query.CountAsync(s => s.Estado == EstadoSolicitud.Pendiente),
            EnProceso = await query.CountAsync(s => s.Estado == EstadoSolicitud.EnProceso),
            Aprobadas = await query.CountAsync(s => s.Estado == EstadoSolicitud.Aprobada),
            Rechazadas = await query.CountAsync(s => s.Estado == EstadoSolicitud.Rechazada),
            Canceladas = await query.CountAsync(s => s.Estado == EstadoSolicitud.Cancelada)
        };

        // Trámites frecuentes
        var tipos = await query
            .GroupBy(s => string.IsNullOrWhiteSpace(s.Titulo) ? "General" : s.Titulo)
            .Select(g => new EstadisticaItemDto
            {
                Etiqueta = g.Key,
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Cantidad)
            .Take(6)
            .ToListAsync();

        resumen.PorTipoTramite = tipos;

        // Tendencia cronológica
        var listaFechas = await query.Select(s => s.FechaCreacion).ToListAsync();
        resumen.TendenciaMensual = [.. listaFechas
            .GroupBy(f => f.ToString("MMM yyyy", new System.Globalization.CultureInfo("es-ES")))
            .Select(g => new EstadisticaItemDto
            {
                Etiqueta = g.Key.ToUpper(),
                Cantidad = g.Count()
            })
            .Take(6)];

        return resumen;
    }
}