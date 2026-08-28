using GestionSolicitudes.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestionSolicitudes.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    // 1. Tablas del Dominio
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<DocumentoAdjunto> DocumentosAdjuntos => Set<DocumentoAdjunto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Crucial para esquemas de Identity

        // 2. Configuración de Entidades y Relaciones
        modelBuilder.Entity<Solicitud>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Solicitante).IsRequired().HasMaxLength(200);

            // Relación 1 a Muchos: Una Solicitud tiene muchos DocumentosAdjuntos
            entity.HasMany(e => e.Documentos)
                  .WithOne(d => d.Solicitud)
                  .HasForeignKey(d => d.SolicitudId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentoAdjunto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TipoDocumento).IsRequired().HasMaxLength(150);
            entity.Property(e => e.NombreArchivoOriginal).IsRequired().HasMaxLength(255);
            entity.Property(e => e.RutaAlmacenamiento).IsRequired();
            entity.Property(e => e.Extension).HasMaxLength(10);
            entity.Property(e => e.Estado).HasMaxLength(50);
        });

        // 3. IDs Fijos para Roles y Usuarios
        var adminRoleId = "b1c2d3e4-1111-2222-3333-444455556666";
        var residenteRoleId = "b1c2d3e4-2222-3333-4444-555566667777";

        var adminUserId = "a1b2c3d4-1111-2222-3333-444455556666";
        var residenteUserId = "a1b2c3d4-2222-3333-4444-555566667777";

        // 4. Seed de Roles
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = adminRoleId,
                Name = "Administrador",
                NormalizedName = "ADMINISTRADOR",
                ConcurrencyStamp = "e1a2b3c4-0000-1111-2222-333344445555"
            },
            new IdentityRole
            {
                Id = residenteRoleId,
                Name = "Residente",
                NormalizedName = "RESIDENTE",
                ConcurrencyStamp = "e1a2b3c4-0000-1111-2222-666677778888"
            }
        );

        // 5. Seed de Usuarios (Hashes fijos para evitar migraciones fantasma)
        var adminUser = new IdentityUser
        {
            Id = adminUserId,
            UserName = "admin@migracion.gob.do",
            NormalizedUserName = "ADMIN@MIGRACION.GOB.DO",
            Email = "admin@migracion.gob.do",
            NormalizedEmail = "ADMIN@MIGRACION.GOB.DO",
            EmailConfirmed = true,
            SecurityStamp = "f1adc49c-8213-4e2b-bbe8-738f1df747f4",
            ConcurrencyStamp = "3692d7cb-b567-45c1-935f-aa9bd4d36821",
            PasswordHash = "AQAAAAIAAYagAAAAEKONXxkz1ICCwSWNRVQk/A5iAiMHamkm2/8F7P8XRx1C4UzNek1PzgXTcQokxPovIA==" // Admin123*
        };

        var residenteUser = new IdentityUser
        {
            Id = residenteUserId,
            UserName = "residente@correo.com",
            NormalizedUserName = "RESIDENTE@CORREO.COM",
            Email = "residente@correo.com",
            NormalizedEmail = "RESIDENTE@CORREO.COM",
            EmailConfirmed = true,
            SecurityStamp = "58f62c31-42b8-462b-bf0c-edbb0b6a6812",
            ConcurrencyStamp = "50005b15-b2a3-4b97-b174-c8e1a7bc6d1e",
            PasswordHash = "AQAAAAIAAYagAAAAEGtHVGd6aB8Ai6gFxToteCdRFIixqzf3xDh2JiYO4H6GzTLXJNhklUAPCjL3tfziqg==" // Residente123*
        };

        modelBuilder.Entity<IdentityUser>().HasData(adminUser, residenteUser);

        // 6. Asignación de Roles a Usuarios
        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                RoleId = adminRoleId,
                UserId = adminUserId
            },
            new IdentityUserRole<string>
            {
                RoleId = residenteRoleId,
                UserId = residenteUserId
            }
        );

        // 7. Seed de Solicitudes Migratorias Iniciales
        modelBuilder.Entity<Solicitud>().HasData(
            new Solicitud
            {
                Id = 1,
                Titulo = "Residencia Temporal por Trabajo (RT-3)",
                Solicitante = "Elys Camila Batista Encarnacion",
                Descripcion = "Solicitud inicial de permiso de trabajo temporal y radicación de expediente.",
                Estado = EstadoSolicitud.Pendiente,
                FechaCreacion = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc),
                Activo = true
            },
            new Solicitud
            {
                Id = 2,
                Titulo = "Renovación de Residencia Temporal",
                Solicitante = "Pedro Martinez Gonzales",
                Descripcion = "Depósito de pasaporte renovado y récord policial para extensión de vigencia.",
                Estado = EstadoSolicitud.EnProceso,
                FechaCreacion = new DateTime(2026, 8, 5, 11, 30, 0, DateTimeKind.Utc),
                Activo = true
            },
            new Solicitud
            {
                Id = 3,
                Titulo = "Cambio a Residencia Definitiva",
                Solicitante = "Leonel Leon Pica Piedra",
                Descripcion = "Cumplimiento de tiempo requerido en categoría temporal para cambio definitivo.",
                Estado = EstadoSolicitud.Aprobada,
                FechaCreacion = new DateTime(2026, 8, 10, 9, 15, 0, DateTimeKind.Utc),
                Activo = true
            },
            new Solicitud
            {
                Id = 4,
                Titulo = "Permiso de Reingreso Múltiple",
                Solicitante = "Giana Pichardo Chihuahua",
                Descripcion = "Permiso de salida y retorno temporal durante proceso de evaluación.",
                Estado = EstadoSolicitud.Pendiente,
                FechaCreacion = new DateTime(2026, 8, 15, 14, 0, 0, DateTimeKind.Utc),
                Activo = true
            },
            new Solicitud
            {
                Id = 5,
                Titulo = "Duplicado de Carnet por Deterioro",
                Solicitante = "Rossi Mosqueta Garcia",
                Descripcion = "Reexpedición física de carnet de residente temporal.",
                Estado = EstadoSolicitud.Rechazada,
                FechaCreacion = new DateTime(2026, 8, 20, 16, 45, 0, DateTimeKind.Utc),
                Activo = true
            }
        );
    }
}