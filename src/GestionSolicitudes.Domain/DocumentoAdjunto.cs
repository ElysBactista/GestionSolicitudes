using System;
using System.Text.Json.Serialization;

namespace GestionSolicitudes.Domain
{
    public enum EstadoDocumento
    {
        EnRevision = 1,
        Aprobado = 2,
        Rechazado = 3
    }

    public class DocumentoAdjunto
    {
        public int Id { get; set; }
        public int SolicitudId { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string NombreArchivoOriginal { get; set; } = string.Empty;
        public string RutaAlmacenamiento { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }

        public EstadoDocumento Estado { get; set; } = EstadoDocumento.EnRevision;
        public string? ComentarioRevision { get; set; }

        public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public Solicitud? Solicitud { get; set; }
    }
}