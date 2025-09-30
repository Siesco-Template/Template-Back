using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MainProject.API.Migrations
{
    /// <inheritdoc />
    public partial class check_constraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Reports_Number",
                table: "Reports",
                sql: "5>0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Reports_Number",
                table: "Reports");
        }
    }
}