using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogProjectMVC2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSmallImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostSmallImageUrl",
                table: "Posts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostSmallImageUrl",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
