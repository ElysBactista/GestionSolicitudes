using System.Collections.Generic;
using GestionSolicitudes.Client.DTOs;

namespace GestionSolicitudes.Client.DTOs
{

    public class DashboardResumenDto
    {
        public int TotalSolicitudes { get; set; }
        public int Pendientes { get; set; }
        public int EnProceso { get; set; }
        public int Aprobadas { get; set; }
        public int Rechazadas { get; set; }
        public int Canceladas { get; set; }
        public List<EstadisticaItemDto> PorTipoTramite { get; set; } = new();
        public List<EstadisticaItemDto> TendenciaMensual { get; set; } = new();
    }

    public class EstadisticaItemDto
    {
        public string Etiqueta { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}

