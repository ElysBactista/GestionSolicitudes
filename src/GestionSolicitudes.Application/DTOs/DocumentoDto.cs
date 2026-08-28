using System;

namespace GestionSolicitudes.Application.DTOs
{
    public class DocumentoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Icono { get; set; } = "bi-file-earmark-pdf";
        public DateTime FechaSubida { get; set; }
        public string Estado { get; set; } = "En Revisión";
        public string UrlDescarga { get; set; } = string.Empty;
    }
}