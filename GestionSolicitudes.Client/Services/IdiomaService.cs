using Microsoft.JSInterop;

namespace GestionSolicitudes.Client.Services
{
    public class IdiomaService
    {
        private readonly IJSRuntime _js;
        public string IdiomaActual { get; private set; } = "ES";

        public event Action? OnIdiomaCambiado;

        public IdiomaService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task InicializarAsync()
        {
            try
            {
                var idiomaGuardado = await _js.InvokeAsync<string>("localStorage.getItem", "app_idioma");
                if (!string.IsNullOrWhiteSpace(idiomaGuardado) && (idiomaGuardado == "ES" || idiomaGuardado == "EN"))
                {
                    IdiomaActual = idiomaGuardado;
                }
            }
            catch { }
        }

        public async Task CambiarIdiomaAsync(string nuevoIdioma)
        {
            if (nuevoIdioma != "ES" && nuevoIdioma != "EN") return;
            if (IdiomaActual == nuevoIdioma) return;

            IdiomaActual = nuevoIdioma;

            try
            {
                await _js.InvokeVoidAsync("localStorage.setItem", "app_idioma", nuevoIdioma);
            }
            catch { }

            OnIdiomaCambiado?.Invoke();
        }

        public bool EsEspanol => IdiomaActual == "ES";
        public bool EsIngles => IdiomaActual == "EN";

        public string T(string clave) => IdiomaActual switch
        {
            "EN" => DiccionarioEn.TryGetValue(clave, out var valorEn) ? valorEn : clave,
            _ => DiccionarioEs.TryGetValue(clave, out var valorEs) ? valorEs : clave
        };

        private static readonly Dictionary<string, string> DiccionarioEs = new()
        {
            // Encabezado
            ["DGM_TITULO"] = "DIRECCIÓN GENERAL DE MIGRACIÓN",
            ["USUARIO_INVITADO"] = "Invitado",
            ["USUARIO_INICIAR_SESION"] = "Iniciar sesión",
            ["USUARIO_CERRAR_SESION"] = "Cerrar sesión",
            ["USUARIO_ROL_ADMIN"] = "Administrador",
            ["USUARIO_ROL_RESIDENTE"] = "Residente",

            // Sidebar (Barra Azul)
            ["SIDEBAR_DASHBOARD"] = "Dashboard",
            ["SIDEBAR_SOLICITUDES"] = "Gestión Solicitudes",
            ["SIDEBAR_REVISION"] = "Revisión",
            ["SIDEBAR_PORTAL_RESIDENTE"] = "Portal Residente",
            ["SIDEBAR_MI_EXPEDIENTE"] = "Mi Expediente",
            ["SIDEBAR_NUEVA_SOLICITUD"] = "Nueva Solicitud",
            ["SIDEBAR_SALIR"] = "Cerrar Sesión",

            // Footer
            ["FOOTER_REPUBLICA"] = "REPÚBLICA DOMINICANA",
            ["FOOTER_DGM"] = "DIRECCIÓN GENERAL DE MIGRACIÓN",
            ["FOOTER_DERECHOS"] = "Todos los derechos reservados. Sistema Oficial de Gestión Migratoria.",
            ["FOOTER_TERMINOS"] = "Términos de Servicio",
            ["FOOTER_PRIVACIDAD"] = "Política de Privacidad",
            ["FOOTER_SOPORTE"] = "Mesa de Ayuda",

            // Portal de Inicio / Home
            ["PORTAL_TITULO"] = "Bienvenido al Portal de Residencia",
            ["PORTAL_SUBTITULO"] = "Este portal ciudadano centraliza todos los trámites de residencia temporal y permanente, diseñado bajo normativas de transparencia y agilidad para gestionar la permanencia legal sin intermediarios.",
            ["PORTAL_BTN_INICIAR"] = "Iniciar Solicitud",
            ["PORTAL_BTN_ESTADO"] = "Consultar Estado"
        };

        private static readonly Dictionary<string, string> DiccionarioEn = new()
        {
            // Encabezado
            ["DGM_TITULO"] = "GENERAL DIRECTORATE OF MIGRATION",
            ["USUARIO_INVITADO"] = "Guest",
            ["USUARIO_INICIAR_SESION"] = "Log in",
            ["USUARIO_CERRAR_SESION"] = "Log out",
            ["USUARIO_ROL_ADMIN"] = "Administrator",
            ["USUARIO_ROL_RESIDENTE"] = "Resident",

            // Sidebar (Barra Azul)
            ["SIDEBAR_DASHBOARD"] = "Dashboard",
            ["SIDEBAR_SOLICITUDES"] = "Applications",
            ["SIDEBAR_REVISION"] = "Review",
            ["SIDEBAR_PORTAL_RESIDENTE"] = "Resident Portal",
            ["SIDEBAR_MI_EXPEDIENTE"] = "My File",
            ["SIDEBAR_NUEVA_SOLICITUD"] = "New Application",
            ["SIDEBAR_SALIR"] = "Log out",

            // Footer
            ["FOOTER_REPUBLICA"] = "DOMINICAN REPUBLIC",
            ["FOOTER_DGM"] = "GENERAL DIRECTORATE OF MIGRATION",
            ["FOOTER_DERECHOS"] = "All rights reserved. Official Migration Management System.",
            ["FOOTER_TERMINOS"] = "Terms of Service",
            ["FOOTER_PRIVACIDAD"] = "Privacy Policy",
            ["FOOTER_SOPORTE"] = "Help Desk",

            // Portal de Inicio / Home
            ["PORTAL_TITULO"] = "Welcome to the Residence Portal",
            ["PORTAL_SUBTITULO"] = "This citizen portal centralizes all temporary and permanent residence procedures, designed under transparency and agility guidelines to manage legal stay without intermediaries.",
            ["PORTAL_BTN_INICIAR"] = "Start Application",
            ["PORTAL_BTN_ESTADO"] = "Check Status"
        };
    }
}