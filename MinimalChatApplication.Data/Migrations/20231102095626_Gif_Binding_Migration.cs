using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinimalChatApplication.Data.Migrations
{
    /// <inheritdoc />
    public partial class Gif_Binding_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GifUrl",
                table: "Messages");

            migrationBuilder.AddColumn<int>(
                name: "GifId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Gifs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GifName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gifs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_GifId",
                table: "Messages",
                column: "GifId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Gifs_GifId",
                table: "Messages",
                column: "GifId",
                principalTable: "Gifs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Gifs_GifId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "Gifs");

            migrationBuilder.DropIndex(
                name: "IX_Messages_GifId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "GifId",
                table: "Messages");

            migrationBuilder.AddColumn<string>(
                name: "GifUrl",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
