namespace GestionSolicitudes.Application.DTOs;

public class DashboardDto
{
    public int TotalSolicitudes { get; set; }
    public int Aprobadas { get; set; }
    public int EnProceso { get; set; }
    public int Pendientes { get; set; }
    public int Canceladas { get; set; }
    public int Rechazadas { get; set; }

    // Estadísticas por Tipo de Trámite y Tendencia Temporal
    public List<EstadisticaItemDto> PorTipoTramite { get; set; } = [];
    public List<EstadisticaItemDto> TendenciaMensual { get; set; } = [];
}

public class EstadisticaItemDto
{
    public string Etiqueta { get; set; } = string.Empty;
    public int Cantidad { get; set; }
} 