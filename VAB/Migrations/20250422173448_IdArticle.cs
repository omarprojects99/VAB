using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class IdArticle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdArticle",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdArticle",
                table: "Articles");
        }
    }
}
