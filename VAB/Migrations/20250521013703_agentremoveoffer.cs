using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class agentremoveoffer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentSaisieId",
                table: "EtatDesOffresArrivee");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgentSaisieId",
                table: "EtatDesOffresArrivee",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
