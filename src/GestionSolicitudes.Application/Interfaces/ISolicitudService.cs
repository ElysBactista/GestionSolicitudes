using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GestionSolicitudes.Application.DTOs;

namespace GestionSolicitudes.Application.Interfaces;

public interface ISolicitudService
{
    Task<(int Total, IEnumerable<SolicitudDto> Solicitudes)> ObtenerTodasAsync(string? busqueda, int numeroPagina, int tamanoPagina);
    Task<List<SolicitudItemDto>> ObtenerFiltradasAsync(DateTime? fechaInicio, DateTime? fechaFin, string? busqueda, string? estado, string? tipoTramite);
    Task<SolicitudDto?> ObtenerPorIdAsync(int id);
    Task<SolicitudDto> CrearAsync(CrearSolicitudDto dto);
    Task<bool> ActualizarAsync(int id, ActualizarSolicitudDto dto);
    Task<bool> CambiarEstadoAsync(int id, ActualizarEstadoDto dto);
    Task<bool> DesactivarAsync(int id);
    Task<DashboardDto> ObtenerResumenDashboardAsync(DateTime? fechaInicio, DateTime? fechaFin, string? busqueda, string? estado, string? tipoTramite);
}