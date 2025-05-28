using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class testfirstetatdepartoffre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EtatDesOffresDepartId",
                table: "ArticleOffres",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EtatDesOffresDepart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    DateEdition = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsValidated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtatDesOffresDepart", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleOffres_EtatDesOffresDepartId",
                table: "ArticleOffres",
                column: "EtatDesOffresDepartId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleOffres_EtatDesOffresDepart_EtatDesOffresDepartId",
                table: "ArticleOffres",
                column: "EtatDesOffresDepartId",
                principalTable: "EtatDesOffresDepart",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleOffres_EtatDesOffresDepart_EtatDesOffresDepartId",
                table: "ArticleOffres");

            migrationBuilder.DropTable(
                name: "EtatDesOffresDepart");

            migrationBuilder.DropIndex(
                name: "IX_ArticleOffres_EtatDesOffresDepartId",
                table: "ArticleOffres");

            migrationBuilder.DropColumn(
                name: "EtatDesOffresDepartId",
                table: "ArticleOffres");
        }
    }
}
