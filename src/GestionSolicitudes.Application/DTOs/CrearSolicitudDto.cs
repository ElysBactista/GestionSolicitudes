using System;
using System.ComponentModel.DataAnnotations;
using GestionSolicitudes.Domain;

namespace GestionSolicitudes.Application.DTOs;

public class CrearSolicitudDto
{
    [Required(ErrorMessage = "El título o tipo de trámite es obligatorio.")]
    [StringLength(150, ErrorMessage = "El título no puede exceder 150 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del solicitante es obligatorio.")]
    [StringLength(200, ErrorMessage = "El solicitante no puede exceder 200 caracteres.")]
    public string Solicitante { get; set; } = string.Empty;

    // Identificador de usuario Identity
    public string? UsuarioId { get; set; }

    // Clasificación de la solicitud
    public TipoSolicitud Tipo { get; set; } = TipoSolicitud.Nueva;
    public CategoriaResidencia Categoria { get; set; } = CategoriaResidencia.Trabajo;

    // Datos Personales
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NumeroPasaporte { get; set; } = string.Empty;
    public string Nacionalidad { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }
    public string Sexo { get; set; } = string.Empty;

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
}