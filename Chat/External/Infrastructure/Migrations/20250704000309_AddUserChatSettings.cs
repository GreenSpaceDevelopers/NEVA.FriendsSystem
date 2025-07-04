using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserChatSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserChatSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsMuted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDisabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChatSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserChatSettings_ChatUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ChatUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChatSettings_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserChatSettingsDisabledUsers",
                columns: table => new
                {
                    DisabledUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserChatSettingsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChatSettingsDisabledUsers", x => new { x.DisabledUserId, x.UserChatSettingsId });
                    table.ForeignKey(
                        name: "FK_UserChatSettingsDisabledUsers_ChatUsers_DisabledUserId",
                        column: x => x.DisabledUserId,
                        principalTable: "ChatUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChatSettingsDisabledUsers_UserChatSettings_UserChatSett~",
                        column: x => x.UserChatSettingsId,
                        principalTable: "UserChatSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserChatSettings_ChatId",
                table: "UserChatSettings",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChatSettings_UserId",
                table: "UserChatSettings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChatSettingsDisabledUsers_UserChatSettingsId",
                table: "UserChatSettingsDisabledUsers",
                column: "UserChatSettingsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserChatSettingsDisabledUsers");

            migrationBuilder.DropTable(
                name: "UserChatSettings");
        }
    }
}
