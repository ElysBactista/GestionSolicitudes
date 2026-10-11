using System;
using System.Collections.Generic;

namespace GestionSolicitudes.Domain;

public class Solicitud
{
    public int Id { get; set; }

    // Identificación y clasificación
    public string Codigo { get; set; } = string.Empty; // Ej. SOL-2026-0001 o #001
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Solicitante { get; set; } = string.Empty; // Nombres + Apellidos normalizados
    public string? UsuarioId { get; set; } // Vinculación con Identity

    public TipoSolicitud Tipo { get; set; } = TipoSolicitud.Nueva;
    public CategoriaResidencia Categoria { get; set; } = CategoriaResidencia.Trabajo;
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

    // Datos Personales
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NumeroPasaporte { get; set; } = string.Empty;
    public string Nacionalidad { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }
    public string Sexo { get; set; } = string.Empty; // "M" o "F"

    // Contacto y Domicilio
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string Municipio { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Calle { get; set; } = string.Empty;

    // Condición Legal
    public bool EsResidenteActual { get; set; } = false;
    public bool PoseeDobleNacionalidad { get; set; } = false;
    public string NumeroResidenciaPrevia { get; set; } = string.Empty;

    // Auditoría y Dictamen Administrativo
    public string? MensajeRevision { get; set; }
    public DateTime? FechaEnvio { get; set; }
    public DateTime? FechaRevision { get; set; }
    public string? RevisadaPorId { get; set; }

    // Control y compatibilidad
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public List<DocumentoAdjunto> Documentos { get; set; } = [];
}