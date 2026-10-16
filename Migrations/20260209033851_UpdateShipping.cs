using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShipping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "District",
                table: "Shippings",
                newName: "Address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Shippings",
                newName: "District");
        }
    }
}
