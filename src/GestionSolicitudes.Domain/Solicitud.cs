using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionSolicitudes.Domain;

public class Solicitud
{
    public int Id {get; set;}
    public string Titulo {get; set;} = string.Empty;
    public string Descripcion {get; set;} = string.Empty;
    public string Solicitante {get; set;} = string.Empty;
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set;}
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; }
    public List<DocumentoAdjunto> Documentos { get; set; } = [];
}
