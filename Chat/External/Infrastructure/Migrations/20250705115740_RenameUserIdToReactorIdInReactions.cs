using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserIdToReactorIdInReactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_PostReaction_PostId",
                table: "PostReaction");

            migrationBuilder.DropIndex(
                name: "IX_CommentReaction_CommentId",
                table: "CommentReaction");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PostReaction");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CommentReaction");

            migrationBuilder.CreateTable(
                name: "MessageReactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReactorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReactionTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageReactions_ChatUsers_ReactorId",
                        column: x => x.ReactorId,
                        principalTable: "ChatUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageReactions_Comment_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MessageReactions_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageReactions_ReactionTypes_ReactionTypeId",
                        column: x => x.ReactionTypeId,
                        principalTable: "ReactionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostReaction_PostId_ReactorId_ReactionTypeId",
                table: "PostReaction",
                columns: new[] { "PostId", "ReactorId", "ReactionTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentReaction_CommentId_ReactorId_ReactionTypeId",
                table: "CommentReaction",
                columns: new[] { "CommentId", "ReactorId", "ReactionTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_CommentId",
                table: "MessageReactions",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_MessageId_ReactorId_ReactionTypeId",
                table: "MessageReactions",
                columns: new[] { "MessageId", "ReactorId", "ReactionTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_ReactionTypeId",
                table: "MessageReactions",
                column: "ReactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_ReactorId",
                table: "MessageReactions",
                column: "ReactorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageReactions");

            migrationBuilder.DropIndex(
                name: "IX_PostReaction_PostId_ReactorId_ReactionTypeId",
                table: "PostReaction");

            migrationBuilder.DropIndex(
                name: "IX_CommentReaction_CommentId_ReactorId_ReactionTypeId",
                table: "CommentReaction");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "PostReaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "CommentReaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Reactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReactorId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reactions_ChatUsers_ReactorId",
                        column: x => x.ReactorId,
                        principalTable: "ChatUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reactions_Comment_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reactions_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reactions_ReactionTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "ReactionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostReaction_PostId",
                table: "PostReaction",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReaction_CommentId",
                table: "CommentReaction",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_CommentId",
                table: "Reactions",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_MessageId",
                table: "Reactions",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_ReactorId",
                table: "Reactions",
                column: "ReactorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_TypeId",
                table: "Reactions",
                column: "TypeId");
        }
    }
}
