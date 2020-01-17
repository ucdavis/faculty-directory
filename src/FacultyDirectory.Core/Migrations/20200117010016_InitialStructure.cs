using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FacultyDirectory.Core.Migrations
{
    public partial class InitialStructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IamId = table.Column<string>(maxLength: 16, nullable: false),
                    Kerberos = table.Column<string>(maxLength: 32, nullable: true),
                    FirstName = table.Column<string>(maxLength: 128, nullable: true),
                    LastName = table.Column<string>(maxLength: 128, nullable: true),
                    FullName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 128, nullable: true),
                    Phone = table.Column<string>(maxLength: 128, nullable: true),
                    Title = table.Column<string>(maxLength: 256, nullable: true),
                    Departments = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 64, nullable: true),
                    Url = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PeopleSources",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(nullable: false),
                    Source = table.Column<string>(maxLength: 64, nullable: false),
                    SourceKey = table.Column<string>(maxLength: 128, nullable: false),
                    Data = table.Column<string>(nullable: true),
                    LastUpdate = table.Column<DateTime>(nullable: true),
                    HasKeywords = table.Column<bool>(nullable: false),
                    HasPubs = table.Column<bool>(nullable: false),
                    HasBio = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeopleSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeopleSources_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SitePeople",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(nullable: false),
                    SiteId = table.Column<int>(nullable: false),
                    PageUid = table.Column<Guid>(nullable: true),
                    Bio = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(maxLength: 128, nullable: true),
                    LastName = table.Column<string>(maxLength: 128, nullable: true),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 128, nullable: true),
                    Phone = table.Column<string>(maxLength: 128, nullable: true),
                    Title = table.Column<string>(maxLength: 256, nullable: true),
                    Departments = table.Column<string>(nullable: true),
                    ShouldSync = table.Column<bool>(nullable: false),
                    LastSync = table.Column<DateTime>(nullable: true),
                    LastUpdate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitePeople", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SitePeople_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SitePeople_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_People_IamId",
                table: "People",
                column: "IamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeopleSources_PersonId",
                table: "PeopleSources",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SitePeople_PersonId",
                table: "SitePeople",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SitePeople_SiteId",
                table: "SitePeople",
                column: "SiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeopleSources");

            migrationBuilder.DropTable(
                name: "SitePeople");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "Sites");
        }
    }
}
