using Microsoft.EntityFrameworkCore.Migrations;

namespace FacultyDirectory.Core.Migrations
{
    public partial class WebsiteAndPluralization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "SitePeople");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "SitePeople");

            migrationBuilder.AddColumn<string>(
                name: "Emails",
                table: "SitePeople",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phones",
                table: "SitePeople",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Websites",
                table: "SitePeople",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Emails",
                table: "SitePeople");

            migrationBuilder.DropColumn(
                name: "Phones",
                table: "SitePeople");

            migrationBuilder.DropColumn(
                name: "Websites",
                table: "SitePeople");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "SitePeople",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "SitePeople",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }
    }
}
