using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacultyDirectory.Core.Migrations
{
    public partial class Pronunciation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PronunciationUid",
                table: "SitePeople",
                type: "uniqueidentifier",
                maxLength: 64,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PronunciationUid",
                table: "SitePeople");
        }
    }
}
