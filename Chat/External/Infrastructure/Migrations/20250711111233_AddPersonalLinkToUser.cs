using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalLinkToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PersonalLink",
                table: "ChatUsers",
                type: "text",
                nullable: false,
                defaultValue: "");
            
            migrationBuilder.Sql("UPDATE \"ChatUsers\" SET \"PersonalLink\" = \"Username\"");

            migrationBuilder.CreateIndex(
                name: "IX_ChatUsers_PersonalLink",
                table: "ChatUsers",
                column: "PersonalLink",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatUsers_PersonalLink",
                table: "ChatUsers");

            migrationBuilder.DropColumn(
                name: "PersonalLink",
                table: "ChatUsers");
        }
    }
}
