using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MainProject.API.Migrations
{
    /// <inheritdoc />
    public partial class table_catalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TableCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CatalogPath = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TableCategories_TableId_CatalogPath",
                table: "TableCategories",
                columns: new[] { "TableId", "CatalogPath" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TableCategories");
        }
    }
}
