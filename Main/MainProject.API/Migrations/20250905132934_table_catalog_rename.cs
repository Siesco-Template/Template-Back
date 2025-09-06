using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MainProject.API.Migrations
{
    /// <inheritdoc />
    public partial class table_catalog_rename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TableCategories",
                table: "TableCategories");

            migrationBuilder.RenameTable(
                name: "TableCategories",
                newName: "TableCatalogs");

            migrationBuilder.RenameIndex(
                name: "IX_TableCategories_TableId_CatalogPath",
                table: "TableCatalogs",
                newName: "IX_TableCatalogs_TableId_CatalogPath");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TableCatalogs",
                table: "TableCatalogs",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TableCatalogs",
                table: "TableCatalogs");

            migrationBuilder.RenameTable(
                name: "TableCatalogs",
                newName: "TableCategories");

            migrationBuilder.RenameIndex(
                name: "IX_TableCatalogs_TableId_CatalogPath",
                table: "TableCategories",
                newName: "IX_TableCategories_TableId_CatalogPath");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TableCategories",
                table: "TableCategories",
                column: "Id");
        }
    }
}
