using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class AddVolAndFeuilleDeLignes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeuilleDeLignes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mois = table.Column<int>(type: "int", nullable: false),
                    Annee = table.Column<int>(type: "int", nullable: false),
                    DateGeneration = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeuilleDeLignes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vols",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Heure = table.Column<TimeSpan>(type: "time", nullable: false),
                    TypeVol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombrePassagers = table.Column<int>(type: "int", nullable: false),
                    FeuilleDeLignesId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vols", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vols_FeuilleDeLignes_FeuilleDeLignesId",
                        column: x => x.FeuilleDeLignesId,
                        principalTable: "FeuilleDeLignes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vols_FeuilleDeLignesId",
                table: "Vols",
                column: "FeuilleDeLignesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vols");

            migrationBuilder.DropTable(
                name: "FeuilleDeLignes");
        }
    }
}
