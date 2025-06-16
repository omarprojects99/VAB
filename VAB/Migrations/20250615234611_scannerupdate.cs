using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class scannerupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExtractedPdfData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DateExtraction = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JsonData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false),
                    ExtractedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractedPdfData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExtractedPdfItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExtractedPdfDataId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ItemDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    ColumnNumber = table.Column<int>(type: "int", nullable: false),
                    RawData = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractedPdfItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtractedPdfItems_ExtractedPdfData_ExtractedPdfDataId",
                        column: x => x.ExtractedPdfDataId,
                        principalTable: "ExtractedPdfData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExtractedPdfData_DateExtraction",
                table: "ExtractedPdfData",
                column: "DateExtraction");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractedPdfData_ExtractedBy",
                table: "ExtractedPdfData",
                column: "ExtractedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractedPdfData_Numero",
                table: "ExtractedPdfData",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExtractedPdfItems_ExtractedPdfDataId",
                table: "ExtractedPdfItems",
                column: "ExtractedPdfDataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtractedPdfItems");

            migrationBuilder.DropTable(
                name: "ExtractedPdfData");
        }
    }
}
