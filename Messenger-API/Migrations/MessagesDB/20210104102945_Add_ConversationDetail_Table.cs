using Microsoft.EntityFrameworkCore.Migrations;

namespace Messenger_API.Migrations.MessagesDB
{
    public partial class Add_ConversationDetail_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationDetails");
        }
    }
}
