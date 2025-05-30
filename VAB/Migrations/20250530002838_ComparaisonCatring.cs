using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class ComparaisonCatring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesComparisons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ComparisonDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalEncaisseArrivee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEncaisseDepart = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEncaisseMatch = table.Column<bool>(type: "bit", nullable: false),
                    EcartPlusArrivee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EcartPlusDepart = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EcartPlusMatch = table.Column<bool>(type: "bit", nullable: false),
                    EcartMoinsArrivee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EcartMoinsDepart = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EcartMoinsMatch = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesComparisons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComparisonArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesComparisonId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    IdArticle = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuantiteVenduArrivee = table.Column<int>(type: "int", nullable: false),
                    QuantiteVenduDepart = table.Column<int>(type: "int", nullable: false),
                    QuantiteVenduMatch = table.Column<bool>(type: "bit", nullable: false),
                    PrixUnitaireArrivee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrixUnitaireDepart = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrixUnitaireMatch = table.Column<bool>(type: "bit", nullable: false),
                    ValeurArrivee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValeurDepart = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValeurMatch = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparisonArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComparisonArticles_SalesComparisons_SalesComparisonId",
                        column: x => x.SalesComparisonId,
                        principalTable: "SalesComparisons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonArticles_SalesComparisonId",
                table: "ComparisonArticles",
                column: "SalesComparisonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComparisonArticles");

            migrationBuilder.DropTable(
                name: "SalesComparisons");
        }
    }
}
