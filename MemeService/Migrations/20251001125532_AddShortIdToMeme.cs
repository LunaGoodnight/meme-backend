using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemeService.Migrations
{
    /// <inheritdoc />
    public partial class AddShortIdToMeme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortId",
                table: "Memes",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Memes_ShortId",
                table: "Memes",
                column: "ShortId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Memes_ShortId",
                table: "Memes");

            migrationBuilder.DropColumn(
                name: "ShortId",
                table: "Memes");
        }
    }
}
