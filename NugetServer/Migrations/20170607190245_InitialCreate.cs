using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NugetServer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Authors = table.Column<string>(nullable: true),
                    Copyright = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Dependencies = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    DevelopmentDependency = table.Column<string>(nullable: true),
                    DownloadCount = table.Column<int>(nullable: false),
                    GalleryDetailsUrl = table.Column<string>(nullable: true),
                    IconUrl = table.Column<string>(nullable: true),
                    Identifier = table.Column<string>(nullable: true),
                    IsAbsoluteLatestVersion = table.Column<bool>(nullable: false),
                    IsLatestVersion = table.Column<bool>(nullable: false),
                    IsPrerelease = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(nullable: true),
                    LastEdited = table.Column<DateTime>(nullable: true),
                    LastUpdated = table.Column<DateTime>(nullable: false),
                    LicenseNames = table.Column<string>(nullable: true),
                    LicenseReportUrl = table.Column<string>(nullable: true),
                    LicenseUrl = table.Column<string>(nullable: true),
                    MinClientVersion = table.Column<string>(nullable: true),
                    NormalizedVersion = table.Column<string>(nullable: true),
                    Owners = table.Column<string>(nullable: true),
                    PackageHash = table.Column<string>(nullable: true),
                    PackageHashAlgorithm = table.Column<string>(nullable: true),
                    PackageSize = table.Column<long>(nullable: false),
                    ProjectUrl = table.Column<string>(nullable: true),
                    Published = table.Column<DateTime>(nullable: false),
                    ReleaseNotes = table.Column<string>(nullable: true),
                    ReportAbuseUrl = table.Column<string>(nullable: true),
                    RequireLicenseAcceptance = table.Column<bool>(nullable: false),
                    Summary = table.Column<string>(nullable: true),
                    Tags = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Version = table.Column<string>(nullable: true),
                    VersionDownloadCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Packages");
        }
    }
}
