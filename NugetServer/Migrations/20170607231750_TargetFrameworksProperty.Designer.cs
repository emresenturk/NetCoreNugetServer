using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using NugetServer.Models;

namespace NugetServer.Migrations
{
    [DbContext(typeof(ApplicationDataContext))]
    [Migration("20170607231750_TargetFrameworksProperty")]
    partial class TargetFrameworksProperty
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("NugetServer.Models.Package", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Authors");

                    b.Property<string>("Copyright");

                    b.Property<DateTime>("Created");

                    b.Property<string>("Dependencies");

                    b.Property<string>("Description");

                    b.Property<string>("DevelopmentDependency");

                    b.Property<int>("DownloadCount");

                    b.Property<string>("GalleryDetailsUrl");

                    b.Property<string>("IconUrl");

                    b.Property<string>("Identifier");

                    b.Property<bool>("IsAbsoluteLatestVersion");

                    b.Property<bool>("IsLatestVersion");

                    b.Property<bool>("IsPrerelease");

                    b.Property<string>("Language");

                    b.Property<DateTime?>("LastEdited");

                    b.Property<DateTime>("LastUpdated");

                    b.Property<string>("LicenseNames");

                    b.Property<string>("LicenseReportUrl");

                    b.Property<string>("LicenseUrl");

                    b.Property<string>("MinClientVersion");

                    b.Property<string>("NormalizedVersion");

                    b.Property<string>("Owners");

                    b.Property<string>("PackageHash");

                    b.Property<string>("PackageHashAlgorithm");

                    b.Property<long>("PackageSize");

                    b.Property<string>("ProjectUrl");

                    b.Property<DateTime>("Published");

                    b.Property<string>("ReleaseNotes");

                    b.Property<string>("ReportAbuseUrl");

                    b.Property<bool>("RequireLicenseAcceptance");

                    b.Property<string>("Summary");

                    b.Property<string>("Tags");

                    b.Property<string>("TargetFrameworks");

                    b.Property<string>("Title");

                    b.Property<string>("Version");

                    b.Property<int>("VersionDownloadCount");

                    b.HasKey("Id");

                    b.ToTable("Packages");
                });
        }
    }
}
