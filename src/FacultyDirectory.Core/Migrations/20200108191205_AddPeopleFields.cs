using Microsoft.EntityFrameworkCore.Migrations;

namespace FacultyDirectory.Core.Migrations
{
    public partial class AddPeopleFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Departments",
                table: "People",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "People",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "People",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "People",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Departments",
                table: "People");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "People");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "People");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "People");
        }
    }
}
