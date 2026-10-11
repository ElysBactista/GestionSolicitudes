using GestionSolicitudes.Domain;

namespace GestionSolicitudes.Application.DTOs;

public class MiSolicitudResumenDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public TipoSolicitud Tipo { get; set; }
    public CategoriaResidencia Categoria { get; set; }
    public EstadoSolicitud Estado { get; set; }
    public string EstadoTexto { get; set; } = string.Empty;
    public DateTime? FechaEnvio { get; set; }
    public bool TieneMensaje { get; set; }
}