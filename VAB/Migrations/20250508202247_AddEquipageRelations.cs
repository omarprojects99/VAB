using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipageRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipages",
                columns: table => new
                {
                    EquipageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CcId = table.Column<int>(type: "int", nullable: false),
                    PncSuperviseurId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipages", x => x.EquipageId);
                    table.ForeignKey(
                        name: "FK_Equipages_Ccs_CcId",
                        column: x => x.CcId,
                        principalTable: "Ccs",
                        principalColumn: "CcId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipages_Pncs_PncSuperviseurId",
                        column: x => x.PncSuperviseurId,
                        principalTable: "Pncs",
                        principalColumn: "PncId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PncEquipages",
                columns: table => new
                {
                    PncId = table.Column<int>(type: "int", nullable: false),
                    EquipageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PncEquipages", x => new { x.PncId, x.EquipageId });
                    table.ForeignKey(
                        name: "FK_PncEquipages_Equipages_EquipageId",
                        column: x => x.EquipageId,
                        principalTable: "Equipages",
                        principalColumn: "EquipageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PncEquipages_Pncs_PncId",
                        column: x => x.PncId,
                        principalTable: "Pncs",
                        principalColumn: "PncId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Equipages_CcId",
                table: "Equipages",
                column: "CcId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipages_PncSuperviseurId",
                table: "Equipages",
                column: "PncSuperviseurId");

            migrationBuilder.CreateIndex(
                name: "IX_PncEquipages_EquipageId",
                table: "PncEquipages",
                column: "EquipageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PncEquipages");

            migrationBuilder.DropTable(
                name: "Equipages");
        }
    }
}
