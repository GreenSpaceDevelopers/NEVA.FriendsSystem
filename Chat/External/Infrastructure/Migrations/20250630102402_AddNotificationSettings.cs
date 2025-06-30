using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "NotificationSettingsId",
                table: "ChatUsers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "NotificationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsTelegramEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsEmailEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsPushEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    NewTournamentPosts = table.Column<bool>(type: "boolean", nullable: false),
                    TournamentUpdates = table.Column<bool>(type: "boolean", nullable: false),
                    TournamentTeamInvites = table.Column<bool>(type: "boolean", nullable: false),
                    TeamChanged = table.Column<bool>(type: "boolean", nullable: false),
                    TeamInvites = table.Column<bool>(type: "boolean", nullable: false),
                    TournamentInvites = table.Column<bool>(type: "boolean", nullable: false),
                    AdminRoleAssigned = table.Column<bool>(type: "boolean", nullable: false),
                    TournamentLobbyInvites = table.Column<bool>(type: "boolean", nullable: false),
                    NewTournamentStep = table.Column<bool>(type: "boolean", nullable: false),
                    TournamentStarted = table.Column<bool>(type: "boolean", nullable: false),
                    NewFriendRequest = table.Column<bool>(type: "boolean", nullable: false),
                    NewMessage = table.Column<bool>(type: "boolean", nullable: false),
                    NewPostComment = table.Column<bool>(type: "boolean", nullable: false),
                    NewCommentReply = table.Column<bool>(type: "boolean", nullable: false),
                    NewCommentReaction = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationSettings_ChatUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ChatUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSettings_UserId",
                table: "NotificationSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "NotificationSettingsId",
                table: "ChatUsers");
        }
    }
}
