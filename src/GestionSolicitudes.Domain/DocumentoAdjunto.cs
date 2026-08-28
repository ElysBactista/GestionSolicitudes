using System.Text.Json.Serialization;

namespace GestionSolicitudes.Domain
{
    public class DocumentoAdjunto
    {
        public int Id { get; set; }
        public int SolicitudId { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string NombreArchivoOriginal { get; set; } = string.Empty;
        public string RutaAlmacenamiento { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }
        public string Estado { get; set; } = "En Revisión";
        public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public Solicitud? Solicitud { get; set; }
    }
}