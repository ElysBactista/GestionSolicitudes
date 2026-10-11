using GestionSolicitudes.Domain;

namespace GestionSolicitudes.Application.Common;

public static class EstadoTextos
{
    public static string De(EstadoSolicitud e) => e switch
    {
        EstadoSolicitud.Pendiente => "Pendiente",
        EstadoSolicitud.EnProceso => "En Proceso",
        EstadoSolicitud.Aprobada => "Aprobada",
        EstadoSolicitud.Rechazada => "Rechazada",
        EstadoSolicitud.Cancelada => "Cancelada",
        EstadoSolicitud.RequiereCorreccion => "Requiere Corrección",
        _ => e.ToString()
    };
}