namespace GestionSolicitudes.Domain;

public enum EstadoSolicitud
{
    Pendiente = 1,
    EnProceso = 2,
    Aprobada = 3,
    Rechazada = 4,
    Cancelada = 5,
    RequiereCorreccion = 6
}

public enum TipoSolicitud
{
    Nueva = 1,
    Renovacion = 2
}

public enum CategoriaResidencia
{
    Trabajo = 1,
    Estudiante = 2,
    Familiar = 3,
    Inversionista = 4
}