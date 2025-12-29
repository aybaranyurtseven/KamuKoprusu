using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KamuKoprusu.Migrations
{
    /// <inheritdoc />
    public partial class AddIconClassToBadge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IconClass",
                table: "Badges",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconClass",
                table: "Badges");
        }
    }
}
