﻿// <auto-generated />
using System;
using FacultyDirectory.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FacultyDirectory.Core.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20200108191205_AddPeopleFields")]
    partial class AddPeopleFields
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.0");

            modelBuilder.Entity("FacultyDirectory.Core.Domain.Person", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Departments")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .HasColumnType("TEXT");

                    b.Property<string>("FullName")
                        .HasColumnType("TEXT");

                    b.Property<string>("IamId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Kerberos")
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Phone")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("IamId")
                        .IsUnique();

                    b.ToTable("People");
                });

            modelBuilder.Entity("FacultyDirectory.Core.Domain.PersonSource", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Data")
                        .HasColumnType("TEXT");

                    b.Property<bool>("HasBio")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HasKeywords")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HasPubs")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastUpdate")
                        .HasColumnType("TEXT");

                    b.Property<int>("PersonId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Source")
                        .HasColumnType("TEXT")
                        .HasMaxLength(64);

                    b.Property<string>("SourceKey")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.ToTable("PeopleSources");
                });

            modelBuilder.Entity("FacultyDirectory.Core.Domain.Site", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(64);

                    b.Property<string>("Url")
                        .HasColumnType("TEXT")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.ToTable("Sites");
                });

            modelBuilder.Entity("FacultyDirectory.Core.Domain.SitePerson", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Bio")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastSync")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastUpdate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<Guid?>("PageUid")
                        .HasColumnType("TEXT");

                    b.Property<int>("PersonId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ShouldSync")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SiteId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.HasIndex("SiteId");

                    b.ToTable("SitePeople");
                });

            modelBuilder.Entity("FacultyDirectory.Core.Domain.SitePersonTag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(64);

                    b.Property<int>("SitePersonId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SiteTagId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Source")
                        .HasColumnType("TEXT")
                        .HasMaxLength(64);

                    b.Property<bool>("Sync")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("SitePersonId");

                    b.HasIndex("SiteTagId");

                    b.ToTable("SitePeopleTags");
                });

            modelBuilder.Entity("FacultyDirectory.Core.Domain.SiteTag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(64);

                    b.Property<int>("SiteId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("TagUid")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("SiteId");

                    b.ToTable("SiteTags");
                });

            modelBuilder.Entity("FacultyDirectory.Core.Domain.PersonSource", b =>
                {
                    b.HasOne("FacultyDirectory.Core.Domain.Person", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("FacultyDirectory.Core.Domain.SitePerson", b =>
                {
                    b.HasOne("FacultyDirectory.Core.Domain.Person", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FacultyDirectory.Core.Domain.Site", "Site")
                        .WithMany()
                        .HasForeignKey("SiteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("FacultyDirectory.Core.Domain.SitePersonTag", b =>
                {
                    b.HasOne("FacultyDirectory.Core.Domain.SitePerson", "SitePerson")
                        .WithMany()
                        .HasForeignKey("SitePersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FacultyDirectory.Core.Domain.SiteTag", "SiteTag")
                        .WithMany()
                        .HasForeignKey("SiteTagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("FacultyDirectory.Core.Domain.SiteTag", b =>
                {
                    b.HasOne("FacultyDirectory.Core.Domain.Site", "Site")
                        .WithMany()
                        .HasForeignKey("SiteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
