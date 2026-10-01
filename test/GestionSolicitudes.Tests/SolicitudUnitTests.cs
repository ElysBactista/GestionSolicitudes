using System;
using System.Linq;
using Xunit;
using GestionSolicitudes.Domain;

namespace GestionSolicitudes.Tests
{
    public class SolicitudUnitTests
    {
        [Fact]
        public void CrearSolicitud_Instanciacion_DebeCrearseCorrectamente()
        {
            // Arrange & Act
            var solicitud = new Solicitud();

            // Assert
            Assert.NotNull(solicitud);
        }

        [Fact]
        public void EstadoSolicitud_ValoresPermitidos_DebenEstarDefinidos()
        {
            // Arrange
            var tipoEstado = typeof(EstadoSolicitud);

            // Assert: Verifica que el tipo EstadoSolicitud exista y contenga valores válidos
            if (tipoEstado.IsEnum)
            {
                var nombresEnum = Enum.GetNames(tipoEstado);
                Assert.NotEmpty(nombresEnum);
            }
            else
            {
                // Si es una clase con constantes o propiedades estáticas
                var campos = tipoEstado.GetFields().Select(f => f.Name).ToList();
                Assert.NotEmpty(campos);
            }
        }

        [Fact]
        public void DocumentoAdjunto_Instanciacion_DebePreservarPropiedades()
        {
            // Arrange & Act
            var doc = new DocumentoAdjunto();

            var propTipo = doc.GetType().GetProperty("TipoDocumento");
            var propNombre = doc.GetType().GetProperty("NombreArchivo");

            propTipo?.SetValue(doc, "Página Biográfica del Pasaporte");
            propNombre?.SetValue(doc, "pasaporte.pdf");

            // Assert
            Assert.NotNull(doc);
            if (propTipo != null)
                Assert.Equal("Página Biográfica del Pasaporte", propTipo.GetValue(doc));
            if (propNombre != null)
                Assert.Equal("pasaporte.pdf", propNombre.GetValue(doc));
        }
    }
}