using System;
using System.Collections.Generic;

namespace GestionSolicitudes.Application.DTOs
{
    public class ExpedienteResidenteResponseDto
    {
        public int Id { get; set; }
        public string NumeroRadicado { get; set; } = string.Empty;
        public string TipoResidencia { get; set; } = string.Empty;
        public string NombreTitular { get; set; } = string.Empty;
        public string NumeroPasaporte { get; set; } = string.Empty;
        public string PaisOrigen { get; set; } = string.Empty;
        public int PasoActual { get; set; }
        public int Estado { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public List<DocumentoDto> Documentos { get; set; } = new();
    }
}