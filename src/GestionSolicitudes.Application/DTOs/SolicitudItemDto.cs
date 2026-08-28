namespace GestionSolicitudes.Application.DTOs
{
    public class SolicitudItemDto
    {
        public int Id { get; set; }
        public string NumeroExpediente { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string NumeroPasaporte { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TipoTramite { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }
}