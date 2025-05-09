using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class EtatDeVenteArrivee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ccs",
                columns: table => new
                {
                    CcId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Matricule = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Base = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    College = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Secteur = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ccs", x => x.CcId);
                });

            migrationBuilder.CreateTable(
                name: "EtatDesVentesArrivee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtatDesVentesArrivee", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pncs",
                columns: table => new
                {
                    PncId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Matricule = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Base = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    College = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Secteur = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pncs", x => x.PncId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ccs");

            migrationBuilder.DropTable(
                name: "EtatDesVentesArrivee");

            migrationBuilder.DropTable(
                name: "Pncs");
        }
    }
}
