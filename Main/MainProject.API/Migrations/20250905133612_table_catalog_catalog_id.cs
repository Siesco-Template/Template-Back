using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MainProject.API.Migrations
{
    /// <inheritdoc />
    public partial class table_catalog_catalog_id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CatalogId",
                table: "TableCatalogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CatalogId",
                table: "TableCatalogs");
        }
    }
}
