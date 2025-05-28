using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class testaddoffrearrivee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleVentes_EtatDesOffresArrivee_EtatDesOffresArriveeId",
                table: "ArticleVentes");

            migrationBuilder.DropIndex(
                name: "IX_ArticleVentes_EtatDesOffresArriveeId",
                table: "ArticleVentes");

            migrationBuilder.DropColumn(
                name: "EtatDesOffresArriveeId",
                table: "ArticleVentes");

            migrationBuilder.AddColumn<string>(
                name: "AgentSaisieId",
                table: "EtatDesOffresArrivee",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateEdition",
                table: "EtatDesOffresArrivee",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Numero",
                table: "EtatDesOffresArrivee",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ArticleOffres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtatDesOffresArriveeId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    QuantiteDotation = table.Column<int>(type: "int", nullable: false),
                    QuantiteComplementaire = table.Column<int>(type: "int", nullable: false),
                    QuantiteCasse = table.Column<int>(type: "int", nullable: false),
                    QuantiteOffert = table.Column<int>(type: "int", nullable: false),
                    RetourConsig = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleOffres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleOffres_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleOffres_EtatDesOffresArrivee_EtatDesOffresArriveeId",
                        column: x => x.EtatDesOffresArriveeId,
                        principalTable: "EtatDesOffresArrivee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleOffres_ArticleId",
                table: "ArticleOffres",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleOffres_EtatDesOffresArriveeId",
                table: "ArticleOffres",
                column: "EtatDesOffresArriveeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleOffres");

            migrationBuilder.DropColumn(
                name: "AgentSaisieId",
                table: "EtatDesOffresArrivee");

            migrationBuilder.DropColumn(
                name: "DateEdition",
                table: "EtatDesOffresArrivee");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "EtatDesOffresArrivee");

            migrationBuilder.AddColumn<int>(
                name: "EtatDesOffresArriveeId",
                table: "ArticleVentes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleVentes_EtatDesOffresArriveeId",
                table: "ArticleVentes",
                column: "EtatDesOffresArriveeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleVentes_EtatDesOffresArrivee_EtatDesOffresArriveeId",
                table: "ArticleVentes",
                column: "EtatDesOffresArriveeId",
                principalTable: "EtatDesOffresArrivee",
                principalColumn: "Id");
        }
    }
}
