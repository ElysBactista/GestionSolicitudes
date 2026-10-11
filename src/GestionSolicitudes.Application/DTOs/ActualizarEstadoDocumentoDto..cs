namespace GestionSolicitudes.Application.DTOs
{
    public class ActualizarEstadoDocumentoDto
    {
        public string NuevoEstado { get; set; } = string.Empty; // "Aprobado", "Rechazado", "En Revisión"
        public string? Comentario { get; set; }
    }
}