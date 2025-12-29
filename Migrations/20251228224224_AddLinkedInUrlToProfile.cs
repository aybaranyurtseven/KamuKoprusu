using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KamuKoprusu.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkedInUrlToProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LinkedInUrl",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkedInUrl",
                table: "Profiles");
        }
    }
}
