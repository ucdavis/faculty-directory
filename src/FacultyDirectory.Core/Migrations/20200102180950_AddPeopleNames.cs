using Microsoft.EntityFrameworkCore.Migrations;

namespace FacultyDirectory.Core.Migrations
{
    public partial class AddPeopleNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "People",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "People",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "People",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_People_IamId",
                table: "People",
                column: "IamId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_People_IamId",
                table: "People");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "People");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "People");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "People");
        }
    }
}
