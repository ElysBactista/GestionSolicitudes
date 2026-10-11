using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GestionSolicitudes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarEsquemaColumnasSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Solicitudes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Solicitudes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Solicitante",
                table: "Solicitudes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Apellidos",
                table: "Solicitudes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Calle",
                table: "Solicitudes",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Categoria",
                table: "Solicitudes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Celular",
                table: "Solicitudes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Solicitudes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Correo",
                table: "Solicitudes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EsResidenteActual",
                table: "Solicitudes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEnvio",
                table: "Solicitudes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Solicitudes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRevision",
                table: "Solicitudes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MensajeRevision",
                table: "Solicitudes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Municipio",
                table: "Solicitudes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nacionalidad",
                table: "Solicitudes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombres",
                table: "Solicitudes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroPasaporte",
                table: "Solicitudes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroResidenciaPrevia",
                table: "Solicitudes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "PoseeDobleNacionalidad",
                table: "Solicitudes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Provincia",
                table: "Solicitudes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RevisadaPorId",
                table: "Solicitudes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sector",
                table: "Solicitudes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sexo",
                table: "Solicitudes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Solicitudes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Solicitudes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "Solicitudes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentosAdjuntos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NombreArchivoOriginal = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RutaAlmacenamiento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    ComentarioRevision = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosAdjuntos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosAdjuntos_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAdjuntos_SolicitudId",
                table: "DocumentosAdjuntos",
                column: "SolicitudId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentosAdjuntos");

            migrationBuilder.DropColumn(
                name: "Apellidos",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Calle",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Celular",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Correo",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "EsResidenteActual",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "FechaEnvio",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "FechaRevision",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "MensajeRevision",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Municipio",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Nacionalidad",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Nombres",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "NumeroPasaporte",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "NumeroResidenciaPrevia",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "PoseeDobleNacionalidad",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Provincia",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "RevisadaPorId",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Sector",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Sexo",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Solicitudes");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Solicitudes");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Solicitudes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Solicitante",
                table: "Solicitudes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.InsertData(
                table: "Solicitudes",
                columns: new[] { "Id", "Activo", "Descripcion", "Estado", "FechaActualizacion", "FechaCreacion", "FechaRegistro", "Solicitante", "Titulo" },
                values: new object[,]
                {
                    { 1, true, "", 1, null, new DateTime(2026, 8, 12, 13, 41, 8, 140, DateTimeKind.Utc).AddTicks(4007), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Elys Camila Batista Encarnacion ", "Instalación de equipo" },
                    { 2, true, "", 1, null, new DateTime(2026, 8, 12, 13, 41, 8, 140, DateTimeKind.Utc).AddTicks(4009), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pedro Martinez Gonzales ", "Mantenimiento de software" },
                    { 3, true, "", 1, null, new DateTime(2026, 8, 12, 13, 41, 8, 140, DateTimeKind.Utc).AddTicks(4010), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Leonel Leon Pica piedra  ", "Actualización de sistema" },
                    { 4, true, "", 1, null, new DateTime(2026, 8, 12, 13, 41, 8, 140, DateTimeKind.Utc).AddTicks(4011), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Giana pichardo Chiguaua ", " Revision de Red " },
                    { 5, true, "", 1, null, new DateTime(2026, 8, 12, 13, 41, 8, 140, DateTimeKind.Utc).AddTicks(4011), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), " Rossi Mosqueta Garcia", " Actualizacion de licenias " }
                });
        }
    }
}
