using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFileIdToAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Services_ChatPictureId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Attachments_AttachmentId",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_ChatUsers_AuthorId",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Comment_ParentCommentId",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Post_PostId",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentReaction_ChatUsers_ReactorId",
                table: "CommentReaction");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentReaction_Comment_CommentId",
                table: "CommentReaction");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentReaction_ReactionTypes_ReactionTypeId",
                table: "CommentReaction");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageReactions_Comment_CommentId",
                table: "MessageReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Attachments_AttachmentId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_Attachments_AttachmentId",
                table: "Post");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_ChatUsers_AuthorId",
                table: "Post");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_Post_OriginalPostId",
                table: "Post");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReaction_ChatUsers_ReactorId",
                table: "PostReaction");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReaction_Post_PostId",
                table: "PostReaction");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReaction_ReactionTypes_ReactionTypeId",
                table: "PostReaction");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Messages_AttachmentId",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostReaction",
                table: "PostReaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Post",
                table: "Post");

            migrationBuilder.DropIndex(
                name: "IX_Post_AttachmentId",
                table: "Post");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommentReaction",
                table: "CommentReaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Comment",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_AttachmentId",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "AttachmentId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AttachmentId",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "AttachmentId",
                table: "Comment");

            migrationBuilder.RenameTable(
                name: "PostReaction",
                newName: "PostReactions");

            migrationBuilder.RenameTable(
                name: "Post",
                newName: "Posts");

            migrationBuilder.RenameTable(
                name: "CommentReaction",
                newName: "CommentReactions");

            migrationBuilder.RenameTable(
                name: "Comment",
                newName: "Comments");

            migrationBuilder.RenameIndex(
                name: "IX_PostReaction_ReactorId",
                table: "PostReactions",
                newName: "IX_PostReactions_ReactorId");

            migrationBuilder.RenameIndex(
                name: "IX_PostReaction_ReactionTypeId",
                table: "PostReactions",
                newName: "IX_PostReactions_ReactionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_PostReaction_PostId_ReactorId_ReactionTypeId",
                table: "PostReactions",
                newName: "IX_PostReactions_PostId_ReactorId_ReactionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Post_OriginalPostId",
                table: "Posts",
                newName: "IX_Posts_OriginalPostId");

            migrationBuilder.RenameIndex(
                name: "IX_Post_AuthorId",
                table: "Posts",
                newName: "IX_Posts_AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_CommentReaction_ReactorId",
                table: "CommentReactions",
                newName: "IX_CommentReactions_ReactorId");

            migrationBuilder.RenameIndex(
                name: "IX_CommentReaction_ReactionTypeId",
                table: "CommentReactions",
                newName: "IX_CommentReactions_ReactionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_CommentReaction_CommentId_ReactorId_ReactionTypeId",
                table: "CommentReactions",
                newName: "IX_CommentReactions_CommentId_ReactorId_ReactionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Comment_PostId",
                table: "Comments",
                newName: "IX_Comments_PostId");

            migrationBuilder.RenameIndex(
                name: "IX_Comment_ParentCommentId",
                table: "Comments",
                newName: "IX_Comments_ParentCommentId");

            migrationBuilder.RenameIndex(
                name: "IX_Comment_AuthorId",
                table: "Comments",
                newName: "IX_Comments_AuthorId");

            migrationBuilder.AddColumn<string>(
                name: "FileId",
                table: "Attachments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostReactions",
                table: "PostReactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Posts",
                table: "Posts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommentReactions",
                table: "CommentReactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Comments",
                table: "Comments",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CommentAttachments",
                columns: table => new
                {
                    CommentsId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentAttachments", x => new { x.CommentsId, x.AttachmentsId });
                    table.ForeignKey(
                        name: "FK_CommentAttachments_Attachments_AttachmentsId",
                        column: x => x.AttachmentsId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentAttachments_Comments_CommentsId",
                        column: x => x.CommentsId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageAttachments",
                columns: table => new
                {
                    MessagesId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageAttachments", x => new { x.MessagesId, x.AttachmentsId });
                    table.ForeignKey(
                        name: "FK_MessageAttachments_Attachments_AttachmentsId",
                        column: x => x.AttachmentsId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageAttachments_Messages_MessagesId",
                        column: x => x.MessagesId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostAttachments",
                columns: table => new
                {
                    PostsId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostAttachments", x => new { x.PostsId, x.AttachmentsId });
                    table.ForeignKey(
                        name: "FK_PostAttachments_Attachments_AttachmentsId",
                        column: x => x.AttachmentsId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostAttachments_Posts_PostsId",
                        column: x => x.PostsId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommentAttachments_AttachmentsId",
                table: "CommentAttachments",
                column: "AttachmentsId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageAttachments_AttachmentsId",
                table: "MessageAttachments",
                column: "AttachmentsId");

            migrationBuilder.CreateIndex(
                name: "IX_PostAttachments_AttachmentsId",
                table: "PostAttachments",
                column: "AttachmentsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Attachments_ChatPictureId",
                table: "Chats",
                column: "ChatPictureId",
                principalTable: "Attachments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CommentReactions_ChatUsers_ReactorId",
                table: "CommentReactions",
                column: "ReactorId",
                principalTable: "ChatUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommentReactions_Comments_CommentId",
                table: "CommentReactions",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommentReactions_ReactionTypes_ReactionTypeId",
                table: "CommentReactions",
                column: "ReactionTypeId",
                principalTable: "ReactionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_ChatUsers_AuthorId",
                table: "Comments",
                column: "AuthorId",
                principalTable: "ChatUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Posts_PostId",
                table: "Comments",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReactions_Comments_CommentId",
                table: "MessageReactions",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostReactions_ChatUsers_ReactorId",
                table: "PostReactions",
                column: "ReactorId",
                principalTable: "ChatUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostReactions_Posts_PostId",
                table: "PostReactions",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostReactions_ReactionTypes_ReactionTypeId",
                table: "PostReactions",
                column: "ReactionTypeId",
                principalTable: "ReactionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_ChatUsers_AuthorId",
                table: "Posts",
                column: "AuthorId",
                principalTable: "ChatUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Posts_OriginalPostId",
                table: "Posts",
                column: "OriginalPostId",
                principalTable: "Posts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Attachments_ChatPictureId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentReactions_ChatUsers_ReactorId",
                table: "CommentReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentReactions_Comments_CommentId",
                table: "CommentReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentReactions_ReactionTypes_ReactionTypeId",
                table: "CommentReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_ChatUsers_AuthorId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Posts_PostId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageReactions_Comments_CommentId",
                table: "MessageReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReactions_ChatUsers_ReactorId",
                table: "PostReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReactions_Posts_PostId",
                table: "PostReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PostReactions_ReactionTypes_ReactionTypeId",
                table: "PostReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_ChatUsers_AuthorId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Posts_OriginalPostId",
                table: "Posts");

            migrationBuilder.DropTable(
                name: "CommentAttachments");

            migrationBuilder.DropTable(
                name: "MessageAttachments");

            migrationBuilder.DropTable(
                name: "PostAttachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Posts",
                table: "Posts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostReactions",
                table: "PostReactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Comments",
                table: "Comments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommentReactions",
                table: "CommentReactions");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "Attachments");

            migrationBuilder.RenameTable(
                name: "Posts",
                newName: "Post");

            migrationBuilder.RenameTable(
                name: "PostReactions",
                newName: "PostReaction");

            migrationBuilder.RenameTable(
                name: "Comments",
                newName: "Comment");

            migrationBuilder.RenameTable(
                name: "CommentReactions",
                newName: "CommentReaction");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_OriginalPostId",
                table: "Post",
                newName: "IX_Post_OriginalPostId");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_AuthorId",
                table: "Post",
                newName: "IX_Post_AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_PostReactions_ReactorId",
                table: "PostReaction",
                newName: "IX_PostReaction_ReactorId");

            migrationBuilder.RenameIndex(
                name: "IX_PostReactions_ReactionTypeId",
                table: "PostReaction",
                newName: "IX_PostReaction_ReactionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_PostReactions_PostId_ReactorId_ReactionTypeId",
                table: "PostReaction",
                newName: "IX_PostReaction_PostId_ReactorId_ReactionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_PostId",
                table: "Comment",
                newName: "IX_Comment_PostId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_ParentCommentId",
                table: "Comment",
                newName: "IX_Comment_ParentCommentId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_AuthorId",
                table: "Comment",
                newName: "IX_Comment_AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_CommentReactions_ReactorId",
                table: "CommentReaction",
                newName: "IX_CommentReaction_ReactorId");

            migrationBuilder.RenameIndex(
                name: "IX_CommentReactions_ReactionTypeId",
                table: "CommentReaction",
                newName: "IX_CommentReaction_ReactionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_CommentReactions_CommentId_ReactorId_ReactionTypeId",
                table: "CommentReaction",
                newName: "IX_CommentReaction_CommentId_ReactorId_ReactionTypeId");

            migrationBuilder.AddColumn<Guid>(
                name: "AttachmentId",
                table: "Messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AttachmentId",
                table: "Post",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AttachmentId",
                table: "Comment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Post",
                table: "Post",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostReaction",
                table: "PostReaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Comment",
                table: "Comment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommentReaction",
                table: "CommentReaction",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_AttachmentId",
                table: "Messages",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_AttachmentId",
                table: "Post",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_AttachmentId",
                table: "Comment",
                column: "AttachmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Services_ChatPictureId",
                table: "Chats",
                column: "ChatPictureId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Attachments_AttachmentId",
                table: "Comment",
                column: "AttachmentId",
                principalTable: "Attachments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_ChatUsers_AuthorId",
                table: "Comment",
                column: "AuthorId",
                principalTable: "ChatUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Comment_ParentCommentId",
                table: "Comment",
                column: "ParentCommentId",
                principalTable: "Comment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Post_PostId",
                table: "Comment",
                column: "PostId",
                principalTable: "Post",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommentReaction_ChatUsers_ReactorId",
                table: "CommentReaction",
                column: "ReactorId",
                principalTable: "ChatUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommentReaction_Comment_CommentId",
                table: "CommentReaction",
                column: "CommentId",
                principalTable: "Comment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommentReaction_ReactionTypes_ReactionTypeId",
                table: "CommentReaction",
                column: "ReactionTypeId",
                principalTable: "ReactionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReactions_Comment_CommentId",
                table: "MessageReactions",
                column: "CommentId",
                principalTable: "Comment",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Attachments_AttachmentId",
                table: "Messages",
                column: "AttachmentId",
                principalTable: "Attachments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Post_Attachments_AttachmentId",
                table: "Post",
                column: "AttachmentId",
                principalTable: "Attachments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Post_ChatUsers_AuthorId",
                table: "Post",
                column: "AuthorId",
                principalTable: "ChatUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Post_Post_OriginalPostId",
                table: "Post",
                column: "OriginalPostId",
                principalTable: "Post",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostReaction_ChatUsers_ReactorId",
                table: "PostReaction",
                column: "ReactorId",
                principalTable: "ChatUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostReaction_Post_PostId",
                table: "PostReaction",
                column: "PostId",
                principalTable: "Post",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostReaction_ReactionTypes_ReactionTypeId",
                table: "PostReaction",
                column: "ReactionTypeId",
                principalTable: "ReactionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
