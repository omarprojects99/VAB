using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class MakeIdArticleRequiredAndUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
            name: "IdArticle",
            table: "Articles",
            type: "int",
            nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_IdArticle",
                table: "Articles",
                column: "IdArticle",
                unique: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
            name: "IX_Articles_IdArticle",
            table: "Articles");

            migrationBuilder.AlterColumn<int>(
                name: "IdArticle",
                table: "Articles",
                type: "int",
                nullable: true);
        }
    }
}
