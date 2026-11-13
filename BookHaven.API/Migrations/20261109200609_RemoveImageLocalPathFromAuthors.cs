using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookHaven.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveImageLocalPathFromAuthors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageLocalPath",
                table: "Authors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageLocalPath",
                table: "Authors",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
