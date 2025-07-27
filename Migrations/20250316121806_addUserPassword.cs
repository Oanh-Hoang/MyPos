using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPos.Migrations
{
    /// <inheritdoc />
    public partial class addUserPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PassWord",
                table: "User",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassWord",
                table: "User");
        }
    }
}
