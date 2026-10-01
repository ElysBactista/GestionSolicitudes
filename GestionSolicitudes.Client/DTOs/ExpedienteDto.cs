namespace GestionSolicitudes.Client.DTOs
{
    // =========================================================================
    // CLASE DTO: ExpedienteDto
    // Descripción: Estructura de datos estándar para transferir información 
    // del expediente y los documentos adjuntos reales.
    // =========================================================================
    public class ExpedienteDto
    {
        public int Id { get; set; }
        public string Radicado { get; set; } = string.Empty;
        public string Titular { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Pasaporte { get; set; } = string.Empty;
        public string Nacionalidad { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string FechaRegistro { get; set; } = string.Empty;
        public string Estado { get; set; } = "Pendiente";

        // --- PROPIEDADES DE ARCHIVOS REALES (BASE64) ---
        public string DocPasaporteNombre { get; set; } = string.Empty;
        public string DocPasaporteData { get; set; } = string.Empty;
        public string DocFotoNombre { get; set; } = string.Empty;
        public string DocFotoData { get; set; } = string.Empty;
        public string DocAntecedentesNombre { get; set; } = string.Empty;
        public string DocAntecedentesData { get; set; } = string.Empty;
        public string DocSoporteNombre { get; set; } = string.Empty;
        public string DocSoporteData { get; set; } = string.Empty;
    }
}
