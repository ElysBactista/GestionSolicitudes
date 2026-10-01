using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using GestionSolicitudes.Client.DTOs;
using Microsoft.JSInterop;

namespace GestionSolicitudes.Client.Services
{
    // =========================================================================
    // SERVICIO: ExpedienteStateService
    // Descripción: Gestiona el estado global de la aplicación y la persistencia.
    // =========================================================================
    public class ExpedienteStateService
    {
        private readonly IJSRuntime _js;

        // MÉTODO: Constructor del servicio con inyección de JavaScript Runtime
        public ExpedienteStateService(IJSRuntime js)
        {
            _js = js;
        }

        // --- PROPIEDADES DE ESTADO TEMPORAL ---
        public bool SolicitudIniciada { get; set; } = false;
        public int Id { get; set; } = 1;
        public string Radicado { get; set; } = "#EXP-2026-0001";
        public string Titular { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Pasaporte { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Nacionalidad { get; set; } = string.Empty;
        public string EstadoGeneral { get; set; } = "Pendiente";

        // --- PROPIEDADES DE DOCUMENTOS EN MEMORIA ---
        public string DocPasaporteNombre { get; set; } = string.Empty;
        public string DocPasaporteData { get; set; } = string.Empty;
        public string DocFotoNombre { get; set; } = string.Empty;
        public string DocFotoData { get; set; } = string.Empty;
        public string DocAntecedentesNombre { get; set; } = string.Empty;
        public string DocAntecedentesData { get; set; } = string.Empty;
        public string DocSoporteNombre { get; set; } = string.Empty;
        public string DocSoporteData { get; set; } = string.Empty;

        public List<DocumentoItemMemoria> Documentos { get; set; } = new();

        // Campo privado que respalda la lista de expedientes
        private List<ExpedienteDto> _listaExpedientes = new();

        // PROPIEDAD: ListaExpedientes (Incluye semilla inicial por defecto)
        public List<ExpedienteDto> ListaExpedientes
        {
            get
            {
                if (_listaExpedientes.Count == 0)
                {
                    _listaExpedientes = new List<ExpedienteDto>
                    {
                        new ExpedienteDto
                        {
                            Id = 1, Radicado = "#001", Titular = "Pedro Pérez", Categoria = "Residencia (Estudiante)",
                            Pasaporte = "P688769", Nacionalidad = "COL", Correo = "pedro@gmail.com", FechaRegistro = "17/09/2026", Estado = "Rechazado",
                            DocPasaporteNombre = "pasaporte_pedro.pdf", DocPasaporteData = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf",
                            DocFotoNombre = "foto_pedro.jpg", DocFotoData = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?q=80&w=800&auto=format&fit=crop"
                        },
                        new ExpedienteDto
                        {
                            Id = 2, Radicado = "#002", Titular = "Pedro Martínez González", Categoria = "Residencia Temporal de Estudiante (RE-1)",
                            Pasaporte = "P1184920", Nacionalidad = "CAN", Correo = "pedro.martinez@can.com", FechaRegistro = "16/09/2026", Estado = "En Revisión",
                            DocPasaporteNombre = "pasaporte_martinez.pdf", DocPasaporteData = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf",
                            DocFotoNombre = "foto_martinez.jpg", DocFotoData = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?q=80&w=800&auto=format&fit=crop"
                        },
                        new ExpedienteDto
                        {
                            Id = 3, Radicado = "#003", Titular = "Leonel Piedra", Categoria = "Residencia por Vínculo Familiar",
                            Pasaporte = "A4491023", Nacionalidad = "ESP", Correo = "leonel.piedra@gmail.com", FechaRegistro = "15/09/2026", Estado = "Aprobado",
                            DocPasaporteNombre = "pasaporte_leonel.pdf", DocPasaporteData = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf",
                            DocFotoNombre = "foto_leonel.jpg", DocFotoData = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?q=80&w=800&auto=format&fit=crop"
                        },
                        new ExpedienteDto
                        {
                            Id = 4, Radicado = "#004", Titular = "Sofía Valdés", Categoria = "Residencia Inversionista / Rentista",
                            Pasaporte = "R9938211", Nacionalidad = "MEX", Correo = "sofia@valdes.com", FechaRegistro = "14/09/2026", Estado = "Sin Documentación"
                        },
                        new ExpedienteDto
                        {
                            Id = 5, Radicado = "#005", Titular = "Carlos Mendoza", Categoria = "Residencia Temporal de Trabajo (RT-3)",
                            Pasaporte = "T8839201", Nacionalidad = "ARG", Correo = "carlos@mendoza.com", FechaRegistro = "13/09/2026", Estado = "Pendiente",
                            DocPasaporteNombre = "pasaporte_carlos.pdf", DocPasaporteData = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf",
                            DocFotoNombre = "foto_carlos.jpg", DocFotoData = "https://images.unsplash.com/photo-1492562080023-ab3db95bfbce?q=80&w=800&auto=format&fit=crop"
                        }
                    };
                }
                return _listaExpedientes;
            }
            set => _listaExpedientes = value;
        }

        // MÉTODO: InicializarAsync (Carga datos persistidos desde localStorage)
        public async Task InicializarAsync()
        {
            try
            {
                var json = await _js.InvokeAsync<string>("localStorage.getItem", "dgm_lista_expedientes");
                if (!string.IsNullOrEmpty(json))
                {
                    var listaGuardada = JsonSerializer.Deserialize<List<ExpedienteDto>>(json);
                    if (listaGuardada != null && listaGuardada.Count > 0)
                    {
                        _listaExpedientes = listaGuardada;
                    }
                }
            }
            catch { }
        }

        // MÉTODO: RegistrarExpedienteAsync (Agrega y persiste de forma asíncrona)
        public async Task RegistrarExpedienteAsync(ExpedienteDto nuevo)
        {
            if (_listaExpedientes == null) _listaExpedientes = new();
            _listaExpedientes.Insert(0, nuevo);
            await PersistirAsync();
        }

        // MÉTODO: RegistrarExpediente (Agrega y dispara persistencia en segundo plano)
        public void RegistrarExpediente(ExpedienteDto nuevo)
        {
            if (_listaExpedientes == null) _listaExpedientes = new();
            _listaExpedientes.Insert(0, nuevo);
            _ = PersistirAsync();
        }

        // MÉTODO: PersistirAsync (Guarda la lista actual en el localStorage)
        public async Task PersistirAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_listaExpedientes);
                await _js.InvokeVoidAsync("localStorage.setItem", "dgm_lista_expedientes", json);
            }
            catch { }
        }

        // MÉTODO: RegistrarDocumento (Registra un archivo temporalmente en memoria)
        public void RegistrarDocumento(string tipo, string nombre, string dataUrl, bool esImagen, string icono)
        {
            Documentos.RemoveAll(d => d.TipoDocumento == tipo);
            Documentos.Add(new DocumentoItemMemoria
            {
                Id = Documentos.Count + 1,
                TipoDocumento = tipo,
                NombreArchivo = nombre,
                UrlPreview = dataUrl,
                EsImagen = esImagen,
                Icono = icono,
                Estado = "Pendiente"
            });
        }
    }

    // CLASE AUXILIAR: DocumentoItemMemoria
    public class DocumentoItemMemoria
    {
        public int Id { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public string UrlPreview { get; set; } = string.Empty;
        public bool EsImagen { get; set; }
        public string Icono { get; set; } = "bi-file-earmark";
        public string Estado { get; set; } = "Pendiente";
        public string MotivoObservacion { get; set; } = string.Empty;
    }
}