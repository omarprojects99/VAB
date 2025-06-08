using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class redevance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Redevances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mois = table.Column<int>(type: "int", nullable: false),
                    Annee = table.Column<int>(type: "int", nullable: false),
                    TotalVentes = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantRedevance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DateCalcul = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NombreEtats = table.Column<int>(type: "int", nullable: false),
                    CalculeParId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsValidated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Redevances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Redevances_AspNetUsers_CalculeParId",
                        column: x => x.CalculeParId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RedevanceDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RedevanceId = table.Column<int>(type: "int", nullable: false),
                    EtatDesVentesArriveeId = table.Column<int>(type: "int", nullable: false),
                    TotalVentes = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantRedevance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NumeroEtat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateEtat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NombreArticles = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedevanceDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedevanceDetails_EtatDesVentesArrivee_EtatDesVentesArriveeId",
                        column: x => x.EtatDesVentesArriveeId,
                        principalTable: "EtatDesVentesArrivee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RedevanceDetails_Redevances_RedevanceId",
                        column: x => x.RedevanceId,
                        principalTable: "Redevances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RedevanceDetails_EtatDesVentesArriveeId",
                table: "RedevanceDetails",
                column: "EtatDesVentesArriveeId");

            migrationBuilder.CreateIndex(
                name: "IX_RedevanceDetails_RedevanceId",
                table: "RedevanceDetails",
                column: "RedevanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Redevances_Annee_Mois",
                table: "Redevances",
                columns: new[] { "Annee", "Mois" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Redevances_CalculeParId",
                table: "Redevances",
                column: "CalculeParId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RedevanceDetails");

            migrationBuilder.DropTable(
                name: "Redevances");
        }
    }
}
