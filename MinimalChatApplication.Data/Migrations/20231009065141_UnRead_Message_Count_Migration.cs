using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinimalChatApplication.Data.Migrations
{
    /// <inheritdoc />
    public partial class UnRead_Message_Count_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UnreadMessageCounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReceiverId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MessageCount = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnreadMessageCounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnreadMessageCounts_AspNetUsers_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnreadMessageCounts_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnreadMessageCounts_ReceiverId",
                table: "UnreadMessageCounts",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_UnreadMessageCounts_SenderId_ReceiverId",
                table: "UnreadMessageCounts",
                columns: new[] { "SenderId", "ReceiverId" },
                unique: true,
                filter: "[SenderId] IS NOT NULL AND [ReceiverId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnreadMessageCounts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");
        }
    }
}
