using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Messenger_API.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    ConversationId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.ConversationId);
                });

            migrationBuilder.CreateTable(
                name: "Friends",
                columns: table => new
                {
                    FriendId = table.Column<string>(nullable: false),
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
                name: "ConversationDetails",
                columns: table => new
                {
                    ConversationId = table.Column<string>(nullable: false),
                    ConversationName = table.Column<string>(nullable: true),
                    isGroup = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationDetails", x => x.ConversationId);
                    table.ForeignKey(
                        name: "FK_ConversationDetails_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "ConversationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Packets",
                columns: table => new
                {
                    PacketId = table.Column<string>(nullable: false),
                    ConversationId = table.Column<string>(nullable: true),
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
                name: "BlockedContact",
                columns: table => new
                {
                    ConversationId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedContact", x => x.ConversationId);
                    table.ForeignKey(
                        name: "FK_BlockedContact_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "ConversationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlockedContact_SmallUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SmallUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConversationMembers",
                columns: table => new
                {
                    ConversationId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    IsAdmin = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationMembers", x => new { x.ConversationId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ConversationMembers_SmallUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SmallUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FriendNames",
                columns: table => new
                {
                    FriendId = table.Column<string>(nullable: false),
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
                name: "ImageProfiles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    Image = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_ImageProfiles_SmallUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SmallUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessageContents",
                columns: table => new
                {
                    MessageId = table.Column<string>(nullable: false),
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
                name: "PacketContents",
                columns: table => new
                {
                    MessageId = table.Column<string>(nullable: false),
                    PacketId = table.Column<string>(nullable: true)
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
                name: "IX_BlockedContact_UserId",
                table: "BlockedContact",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMembers_UserId",
                table: "ConversationMembers",
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
                name: "BlockedContact");

            migrationBuilder.DropTable(
                name: "ConversationDetails");

            migrationBuilder.DropTable(
                name: "ConversationMembers");

            migrationBuilder.DropTable(
                name: "FriendNames");

            migrationBuilder.DropTable(
                name: "ImageProfiles");

            migrationBuilder.DropTable(
                name: "PacketContents");

            migrationBuilder.DropTable(
                name: "Friends");

            migrationBuilder.DropTable(
                name: "MessageContents");

            migrationBuilder.DropTable(
                name: "Packets");

            migrationBuilder.DropTable(
                name: "SmallUsers");

            migrationBuilder.DropTable(
                name: "Conversations");
        }
    }
}
