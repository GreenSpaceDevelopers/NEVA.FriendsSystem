using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "ChatUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "ChatUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ChatUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Surname",
                table: "ChatUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "ChatUsers");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "ChatUsers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ChatUsers");

            migrationBuilder.DropColumn(
                name: "Surname",
                table: "ChatUsers");
        }
    }
}
