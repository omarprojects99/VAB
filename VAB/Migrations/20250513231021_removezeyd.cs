using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class removezeyd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.DropColumn(
                name: "PrixUnitaireHT",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "QuantiteCasse",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "QuantiteComplementaire",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "QuantiteDotation",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "QuantiteOffert",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "QuantiteVendu",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Restant",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "TotalEmbarque",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Valeur",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "QuantiteCasse",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "QuantiteComplementaire",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "QuantiteDotation",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "QuantiteOffert",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "QuantiteVendu",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "Restant",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "TotalEmbarque",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "Valeur",
                table: "Articles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<decimal>(
                name: "PrixUnitaireHT",
                table: "Categories",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "QuantiteCasse",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantiteComplementaire",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantiteDotation",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantiteOffert",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantiteVendu",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Restant",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalEmbarque",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Valeur",
                table: "Categories",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "QuantiteCasse",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantiteComplementaire",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantiteDotation",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantiteOffert",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantiteVendu",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Restant",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalEmbarque",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Valeur",
                table: "Articles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

          
        }
    }
}
