using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using GestionSolicitudes.Infrastructure.Persistence;
using GestionSolicitudes.Domain;

namespace GestionSolicitudes.Tests
{
    public class SolicitudIntegrationTests
    {
        private ApplicationDbContext CrearContextoEnMemoria()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task BaseDeDatosEnMemoria_PuedeInicializarseYPersistirEntidad()
        {
            // Arrange
            using var context = CrearContextoEnMemoria();
            var solicitud = new Solicitud();

            // Act
            context.Set<Solicitud>().Add(solicitud);
            var guardados = await context.SaveChangesAsync();

            // Assert
            Assert.True(guardados > 0);
            var total = await context.Set<Solicitud>().CountAsync();
            Assert.Equal(1, total);
        }

        [Fact]
        public async Task BaseDeDatosEnMemoria_AislamientoEntrePruebas()
        {
            // Cada test genera su propia base de datos aislada con Guid único
            using var context = CrearContextoEnMemoria();
            var total = await context.Set<Solicitud>().CountAsync();

            Assert.Equal(0, total);
        }
    }
}