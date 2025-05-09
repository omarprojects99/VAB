using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipageToEtatDesVentesArrivee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipages_Ccs_CcId",
                table: "Equipages");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipages_Pncs_PncSuperviseurId",
                table: "Equipages");

            migrationBuilder.DropColumn(
                name: "DateCreation",
                table: "Equipages");

            migrationBuilder.RenameColumn(
                name: "PncSuperviseurId",
                table: "Equipages",
                newName: "Pnc1Id");

            migrationBuilder.RenameColumn(
                name: "CcId",
                table: "Equipages",
                newName: "Cc1Id");

            migrationBuilder.RenameIndex(
                name: "IX_Equipages_PncSuperviseurId",
                table: "Equipages",
                newName: "IX_Equipages_Pnc1Id");

            migrationBuilder.RenameIndex(
                name: "IX_Equipages_CcId",
                table: "Equipages",
                newName: "IX_Equipages_Cc1Id");

            migrationBuilder.AddColumn<int>(
                name: "EquipageId",
                table: "EtatDesVentesArrivee",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Cc2Id",
                table: "Equipages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Pnc2Id",
                table: "Equipages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EtatDesVentesArrivee_EquipageId",
                table: "EtatDesVentesArrivee",
                column: "EquipageId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipages_Cc2Id",
                table: "Equipages",
                column: "Cc2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Equipages_Pnc2Id",
                table: "Equipages",
                column: "Pnc2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipages_Ccs_Cc1Id",
                table: "Equipages",
                column: "Cc1Id",
                principalTable: "Ccs",
                principalColumn: "CcId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipages_Ccs_Cc2Id",
                table: "Equipages",
                column: "Cc2Id",
                principalTable: "Ccs",
                principalColumn: "CcId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipages_Pncs_Pnc1Id",
                table: "Equipages",
                column: "Pnc1Id",
                principalTable: "Pncs",
                principalColumn: "PncId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipages_Pncs_Pnc2Id",
                table: "Equipages",
                column: "Pnc2Id",
                principalTable: "Pncs",
                principalColumn: "PncId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EtatDesVentesArrivee_Equipages_EquipageId",
                table: "EtatDesVentesArrivee",
                column: "EquipageId",
                principalTable: "Equipages",
                principalColumn: "EquipageId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipages_Ccs_Cc1Id",
                table: "Equipages");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipages_Ccs_Cc2Id",
                table: "Equipages");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipages_Pncs_Pnc1Id",
                table: "Equipages");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipages_Pncs_Pnc2Id",
                table: "Equipages");

            migrationBuilder.DropForeignKey(
                name: "FK_EtatDesVentesArrivee_Equipages_EquipageId",
                table: "EtatDesVentesArrivee");

            migrationBuilder.DropIndex(
                name: "IX_EtatDesVentesArrivee_EquipageId",
                table: "EtatDesVentesArrivee");

            migrationBuilder.DropIndex(
                name: "IX_Equipages_Cc2Id",
                table: "Equipages");

            migrationBuilder.DropIndex(
                name: "IX_Equipages_Pnc2Id",
                table: "Equipages");

            migrationBuilder.DropColumn(
                name: "EquipageId",
                table: "EtatDesVentesArrivee");

            migrationBuilder.DropColumn(
                name: "Cc2Id",
                table: "Equipages");

            migrationBuilder.DropColumn(
                name: "Pnc2Id",
                table: "Equipages");

            migrationBuilder.RenameColumn(
                name: "Pnc1Id",
                table: "Equipages",
                newName: "PncSuperviseurId");

            migrationBuilder.RenameColumn(
                name: "Cc1Id",
                table: "Equipages",
                newName: "CcId");

            migrationBuilder.RenameIndex(
                name: "IX_Equipages_Pnc1Id",
                table: "Equipages",
                newName: "IX_Equipages_PncSuperviseurId");

            migrationBuilder.RenameIndex(
                name: "IX_Equipages_Cc1Id",
                table: "Equipages",
                newName: "IX_Equipages_CcId");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreation",
                table: "Equipages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_Equipages_Ccs_CcId",
                table: "Equipages",
                column: "CcId",
                principalTable: "Ccs",
                principalColumn: "CcId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipages_Pncs_PncSuperviseurId",
                table: "Equipages",
                column: "PncSuperviseurId",
                principalTable: "Pncs",
                principalColumn: "PncId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
