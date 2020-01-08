using Microsoft.EntityFrameworkCore.Migrations;

namespace FacultyDirectory.Core.Migrations
{
    public partial class AddSitePersonFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Departments",
                table: "SitePeople",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "SitePeople",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "SitePeople",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "SitePeople",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "SitePeople",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "SitePeople",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Departments",
                table: "SitePeople");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "SitePeople");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "SitePeople");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "SitePeople");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "SitePeople");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "SitePeople");
        }
    }
}
