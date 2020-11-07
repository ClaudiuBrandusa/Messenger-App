using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Messenger_API.Migrations.MessagesDB
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Friends",
                columns: table => new
                {
                    FriendId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SentDate = table.Column<DateTime>(nullable: false),
                    ConfirmedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friends", x => x.FriendId);
                });

            migrationBuilder.CreateTable(
                name: "SmallUsers",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmallUsers", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    ConversationId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    IsAdmin = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => new { x.ConversationId, x.UserId });
                    table.UniqueConstraint("AK_Conversations_ConversationId", x => x.ConversationId);
                    table.ForeignKey(
                        name: "FK_Conversations_SmallUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SmallUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FriendNames",
                columns: table => new
                {
                    FriendId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendNames", x => new { x.FriendId, x.UserId });
                    table.ForeignKey(
                        name: "FK_FriendNames_Friends_FriendId",
                        column: x => x.FriendId,
                        principalTable: "Friends",
                        principalColumn: "FriendId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FriendNames_SmallUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SmallUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessageContents",
                columns: table => new
                {
                    MessageId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true),
                    SentDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageContents", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_MessageContents_SmallUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SmallUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Packets",
                columns: table => new
                {
                    PacketId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(nullable: false),
                    PacketNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packets", x => x.PacketId);
                    table.ForeignKey(
                        name: "FK_Packets_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "ConversationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PacketContents",
                columns: table => new
                {
                    MessageId = table.Column<int>(nullable: false),
                    PacketId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PacketContents", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_PacketContents_MessageContents_MessageId",
                        column: x => x.MessageId,
                        principalTable: "MessageContents",
                        principalColumn: "MessageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PacketContents_Packets_PacketId",
                        column: x => x.PacketId,
                        principalTable: "Packets",
                        principalColumn: "PacketId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_UserId",
                table: "Conversations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendNames_UserId",
                table: "FriendNames",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageContents_UserId",
                table: "MessageContents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PacketContents_PacketId",
                table: "PacketContents",
                column: "PacketId");

            migrationBuilder.CreateIndex(
                name: "IX_Packets_ConversationId",
                table: "Packets",
                column: "ConversationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FriendNames");

            migrationBuilder.DropTable(
                name: "PacketContents");

            migrationBuilder.DropTable(
                name: "Friends");

            migrationBuilder.DropTable(
                name: "MessageContents");

            migrationBuilder.DropTable(
                name: "Packets");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "SmallUsers");
        }
    }
}
