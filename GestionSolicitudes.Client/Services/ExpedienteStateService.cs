using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using GestionSolicitudes.Client.DTOs;

namespace GestionSolicitudes.Client.Services
{
    public class ExpedienteStateService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public List<ExpedienteDto> ListaExpedientes { get; set; } = new();

        public bool SolicitudIniciada { get; set; } = false;
        public string Radicado { get; set; } = string.Empty;
        public string Titular { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Pasaporte { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Nacionalidad { get; set; } = string.Empty;
        public string EstadoGeneral { get; set; } = "Pendiente";

        public string DocPasaporteNombre { get; set; } = string.Empty;
        public string DocPasaporteData { get; set; } = string.Empty;
        public string DocFotoNombre { get; set; } = string.Empty;
        public string DocFotoData { get; set; } = string.Empty;
        public string DocAntecedentesNombre { get; set; } = string.Empty;
        public string DocAntecedentesData { get; set; } = string.Empty;
        public string DocSoporteNombre { get; set; } = string.Empty;
        public string DocSoporteData { get; set; } = string.Empty;

        public List<DocumentoMemoriaDto> Documentos { get; set; } = new();

        public ExpedienteStateService(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }

        public async Task InicializarAsync()
        {
            try
            {
                // Solicitamos a la API garantizando un tamaño de página suficiente
                var response = await _http.GetAsync("api/solicitud?tamanoPagina=50");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    // Solución K1: Obtener el array desde la propiedad "datos", o la raíz si fuera array
                    JsonElement arrayElement;
                    if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("datos", out var datosProp))
                    {
                        arrayElement = datosProp;
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        arrayElement = doc.RootElement;
                    }
                    else
                    {
                        ListaExpedientes = new List<ExpedienteDto>();
                        return;
                    }

                    var lista = new List<ExpedienteDto>();

                    foreach (var elem in arrayElement.EnumerateArray())
                    {
                        int id = elem.TryGetProperty("id", out var pId) ? pId.GetInt32() : 0;
                        string titulo = elem.TryGetProperty("titulo", out var pTit) ? pTit.GetString() ?? "" : "";
                        string solicitante = elem.TryGetProperty("solicitante", out var pSol) ? pSol.GetString() ?? "" : "";
                        string descripcion = elem.TryGetProperty("descripcion", out var pDesc) ? pDesc.GetString() ?? "" : "";

                        // Parse flexible de fecha
                        string fecha = DateTime.Now.ToString("dd/MM/yyyy");
                        if (elem.TryGetProperty("fechaCreacion", out var pFecha))
                        {
                            if (pFecha.TryGetDateTime(out var dt))
                                fecha = dt.ToString("dd/MM/yyyy");
                            else
                                fecha = pFecha.GetString() ?? fecha;
                        }

                        // Mapeo flexible del estado
                        string estadoTexto = "Pendiente";
                        if (elem.TryGetProperty("estado", out var pEst))
                        {
                            if (pEst.ValueKind == JsonValueKind.Number)
                            {
                                estadoTexto = pEst.GetInt32() switch
                                {
                                    1 => "Pendiente",
                                    2 => "En Proceso",
                                    3 => "Aprobado",
                                    4 => "Rechazado",
                                    5 => "Cancelado",
                                    _ => "Pendiente"
                                };
                            }
                            else if (pEst.ValueKind == JsonValueKind.String)
                            {
                                estadoTexto = pEst.GetString() ?? "Pendiente";
                            }
                        }

                        // Extracción de datos auxiliares
                        string pasaporte = elem.TryGetProperty("pasaporte", out var pPas) ? pPas.GetString() ?? "" : "";
                        string correo = elem.TryGetProperty("correo", out var pCor) ? pCor.GetString() ?? "" : "";

                        if (string.IsNullOrWhiteSpace(pasaporte) && descripcion.Contains("Pasaporte:"))
                        {
                            var partes = descripcion.Split('-');
                            foreach (var part in partes)
                            {
                                if (part.Contains("Pasaporte:")) pasaporte = part.Replace("Pasaporte:", "").Trim();
                                if (part.Contains("Correo:")) correo = part.Replace("Correo:", "").Trim();
                            }
                        }

                        lista.Add(new ExpedienteDto
                        {
                            Id = id,
                            Radicado = $"#{id:D3}",
                            Titular = string.IsNullOrWhiteSpace(solicitante) ? "Sin Solicitante" : solicitante.Trim(),
                            Categoria = string.IsNullOrWhiteSpace(titulo) ? "Residencia Temporal" : titulo.Trim(),
                            Pasaporte = string.IsNullOrWhiteSpace(pasaporte) ? "N/A" : pasaporte,
                            FechaRegistro = fecha,
                            Estado = estadoTexto,
                            Correo = correo
                        });
                    }

                    ListaExpedientes = lista;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al consultar api/solicitud: {ex.Message}");
            }
        }

        public async Task ActualizarEstadoExpedienteAsync(int id, string nuevoEstado)
        {
            int estadoEnum = nuevoEstado switch
            {
                "Aprobado" => 3,
                "En Proceso" or "En Revisión" => 2,
                "Rechazado" => 4,
                "Cancelado" => 5,
                _ => 1
            };

            try
            {
                await _http.PatchAsJsonAsync($"api/solicitud/{id}/estado", new { NuevoEstado = estadoEnum });
            }
            catch { }

            var item = ListaExpedientes.FirstOrDefault(e => e.Id == id);
            if (item != null)
            {
                item.Estado = nuevoEstado;
            }
        }

        public void RegistrarDocumento(string tipo, string nombre, string dataUrl, bool esImg, string icono)
        {
            Documentos.RemoveAll(d => d.TipoDocumento == tipo);
            int nuevoId = Documentos.Count > 0 ? Documentos.Max(d => d.Id) + 1 : 1;

            Documentos.Add(new DocumentoMemoriaDto
            {
                Id = nuevoId,
                TipoDocumento = tipo,
                NombreArchivo = nombre,
                UrlPreview = dataUrl,
                EsImagen = esImg,
                Icono = icono,
                Estado = "Pendiente"
            });
        }
    }

    public class DocumentoMemoriaDto
    {
        public int Id { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public string UrlPreview { get; set; } = string.Empty;
        public bool EsImagen { get; set; } = true;
        public string Icono { get; set; } = string.Empty;
        public string Estado { get; set; } = "Pendiente";
    }
}