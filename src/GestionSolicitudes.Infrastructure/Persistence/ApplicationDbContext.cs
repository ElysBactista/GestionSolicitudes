using System;
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
        base.OnModelCreating(modelBuilder);

        // 2. Configuración de Entidades y Relaciones
        modelBuilder.Entity<Solicitud>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Solicitante).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Nombres).HasMaxLength(100);
            entity.Property(e => e.Apellidos).HasMaxLength(100);
            entity.Property(e => e.NumeroPasaporte).HasMaxLength(50);
            entity.Property(e => e.Nacionalidad).HasMaxLength(10);
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.Celular).HasMaxLength(30);
            entity.Property(e => e.Provincia).HasMaxLength(100);
            entity.Property(e => e.Municipio).HasMaxLength(100);
            entity.Property(e => e.Sector).HasMaxLength(150);
            entity.Property(e => e.Calle).HasMaxLength(255);

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
            entity.Property(e => e.ComentarioRevision).HasMaxLength(500);
        });
    }
}