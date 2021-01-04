using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Messenger_API.Migrations.MessagesDB
{
    public partial class ChangeNameTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Profiles");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageProfiles");

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProfileImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Profiles_SmallUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SmallUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });
        }
    }
}
