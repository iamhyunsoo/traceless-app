using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class EditUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ChatRooms_ChatRoomId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ChatRoomId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ChatRoomId",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "ApplicationUserChatRoom",
                columns: table => new
                {
                    ChatRoomsId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsersId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserChatRoom", x => new { x.ChatRoomsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserChatRoom_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserChatRoom_ChatRooms_ChatRoomsId",
                        column: x => x.ChatRoomsId,
                        principalTable: "ChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserChatRoom_UsersId",
                table: "ApplicationUserChatRoom",
                column: "UsersId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserChatRoom");

            migrationBuilder.AddColumn<string>(
                name: "ChatRoomId",
                table: "AspNetUsers",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ChatRoomId",
                table: "AspNetUsers",
                column: "ChatRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ChatRooms_ChatRoomId",
                table: "AspNetUsers",
                column: "ChatRoomId",
                principalTable: "ChatRooms",
                principalColumn: "Id");
        }
    }
}
