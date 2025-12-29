using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KamuKoprusu.Migrations
{
    /// <inheritdoc />
    public partial class FixBannedUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BannedEmail",
                table: "BannedUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BannedPhone",
                table: "BannedUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnbannedAt",
                table: "BannedUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannedEmail",
                table: "BannedUsers");

            migrationBuilder.DropColumn(
                name: "BannedPhone",
                table: "BannedUsers");

            migrationBuilder.DropColumn(
                name: "UnbannedAt",
                table: "BannedUsers");
        }
    }
}
