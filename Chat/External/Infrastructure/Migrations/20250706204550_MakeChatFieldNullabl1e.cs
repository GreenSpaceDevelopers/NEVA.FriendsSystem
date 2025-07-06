using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeChatFieldNullabl1e : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Services_ChatPictureId",
                table: "Chats");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Services_ChatPictureId",
                table: "Chats",
                column: "ChatPictureId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Services_ChatPictureId",
                table: "Chats");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Services_ChatPictureId",
                table: "Chats",
                column: "ChatPictureId",
                principalTable: "Services",
                principalColumn: "Id");
        }
    }
}
