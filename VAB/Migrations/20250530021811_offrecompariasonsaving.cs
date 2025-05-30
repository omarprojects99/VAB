using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class offrecompariasonsaving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OffersComparisons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ComparisonDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OffersComparisons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComparisonOfferArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OffersComparisonId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    IdArticle = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuantiteDotationArrivee = table.Column<int>(type: "int", nullable: false),
                    QuantiteDotationDepart = table.Column<int>(type: "int", nullable: false),
                    QuantiteDotationMatch = table.Column<bool>(type: "bit", nullable: false),
                    QuantiteComplementaireArrivee = table.Column<int>(type: "int", nullable: false),
                    QuantiteComplementaireDepart = table.Column<int>(type: "int", nullable: false),
                    QuantiteComplementaireMatch = table.Column<bool>(type: "bit", nullable: false),
                    QuantiteCasseArrivee = table.Column<int>(type: "int", nullable: false),
                    QuantiteCasseDepart = table.Column<int>(type: "int", nullable: false),
                    QuantiteCasseMatch = table.Column<bool>(type: "bit", nullable: false),
                    QuantiteOffertArrivee = table.Column<int>(type: "int", nullable: false),
                    QuantiteOffertDepart = table.Column<int>(type: "int", nullable: false),
                    QuantiteOffertMatch = table.Column<bool>(type: "bit", nullable: false),
                    RetourConsigArrivee = table.Column<int>(type: "int", nullable: false),
                    RetourConsigDepart = table.Column<int>(type: "int", nullable: false),
                    RetourConsigMatch = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparisonOfferArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComparisonOfferArticles_OffersComparisons_OffersComparisonId",
                        column: x => x.OffersComparisonId,
                        principalTable: "OffersComparisons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonOfferArticles_OffersComparisonId",
                table: "ComparisonOfferArticles",
                column: "OffersComparisonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComparisonOfferArticles");

            migrationBuilder.DropTable(
                name: "OffersComparisons");
        }
    }
}
