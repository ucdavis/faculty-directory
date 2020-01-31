using Microsoft.EntityFrameworkCore.Migrations;

namespace FacultyDirectory.Core.Migrations
{
    public partial class AddClassification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Classification",
                table: "People",
                maxLength: 16,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Classification",
                table: "People");
        }
    }
}
