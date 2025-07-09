using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLastReadMessageIdToUserChatSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LastReadMessageId",
                table: "UserChatSettings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserChatSettings_LastReadMessageId",
                table: "UserChatSettings",
                column: "LastReadMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserChatSettings_Messages_LastReadMessageId",
                table: "UserChatSettings",
                column: "LastReadMessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserChatSettings_Messages_LastReadMessageId",
                table: "UserChatSettings");

            migrationBuilder.DropIndex(
                name: "IX_UserChatSettings_LastReadMessageId",
                table: "UserChatSettings");

            migrationBuilder.DropColumn(
                name: "LastReadMessageId",
                table: "UserChatSettings");
        }
    }
}
