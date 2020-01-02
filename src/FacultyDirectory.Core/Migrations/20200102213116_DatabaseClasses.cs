using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FacultyDirectory.Core.Migrations
{
    public partial class DatabaseClasses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeopleSources",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonId = table.Column<int>(nullable: false),
                    Source = table.Column<string>(maxLength: 64, nullable: true),
                    SourceKey = table.Column<string>(nullable: true),
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
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(maxLength: 64, nullable: true),
                    Url = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SitePeople",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonId = table.Column<int>(nullable: false),
                    SiteId = table.Column<int>(nullable: false),
                    PageUid = table.Column<Guid>(nullable: true),
                    Bio = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
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

            migrationBuilder.CreateTable(
                name: "SiteTags",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SiteId = table.Column<int>(nullable: false),
                    TagUid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteTags_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SitePeopleTags",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SitePersonId = table.Column<int>(nullable: false),
                    SiteTagId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 64, nullable: true),
                    Source = table.Column<string>(maxLength: 64, nullable: true),
                    Sync = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitePeopleTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SitePeopleTags_SitePeople_SitePersonId",
                        column: x => x.SitePersonId,
                        principalTable: "SitePeople",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SitePeopleTags_SiteTags_SiteTagId",
                        column: x => x.SiteTagId,
                        principalTable: "SiteTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_SitePeopleTags_SitePersonId",
                table: "SitePeopleTags",
                column: "SitePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SitePeopleTags_SiteTagId",
                table: "SitePeopleTags",
                column: "SiteTagId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteTags_SiteId",
                table: "SiteTags",
                column: "SiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeopleSources");

            migrationBuilder.DropTable(
                name: "SitePeopleTags");

            migrationBuilder.DropTable(
                name: "SitePeople");

            migrationBuilder.DropTable(
                name: "SiteTags");

            migrationBuilder.DropTable(
                name: "Sites");
        }
    }
}
