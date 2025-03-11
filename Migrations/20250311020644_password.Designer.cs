﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VotingApp.Data;

#nullable disable

namespace VotingApp.Migrations
{
    [DbContext(typeof(VotingDbContext))]
    [Migration("20250311020644_password")]
    partial class password
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("VotingApp.Models.Candidate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ElectionId")
                        .HasColumnType("int");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ElectionId");

                    b.HasIndex("UserId");

                    b.ToTable("Candidates");
                });

            modelBuilder.Entity("VotingApp.Models.Election", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Elections");
                });

            modelBuilder.Entity("VotingApp.Models.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Roles");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Admin"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Candidate"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Voter"
                        });
                });

            modelBuilder.Entity("VotingApp.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NIC")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Email = "admin@votingapp.com",
                            NIC = "1234567890",
                            Name = "Admin User",
                            PasswordHash = "6G94qKPK8LYNjnTllCqm2G3BUM08AzOK7yW30tfjrMc="
                        });
                });

            modelBuilder.Entity("VotingApp.Models.UserRole", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles");

                    b.HasData(
                        new
                        {
                            UserId = 1,
                            RoleId = 1
                        });
                });

            modelBuilder.Entity("VotingApp.Models.Vote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CandidateId")
                        .HasColumnType("int");

                    b.Property<int>("ElectionId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<DateTime>("VoteTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("CandidateId");

                    b.HasIndex("ElectionId");

                    b.HasIndex("UserId", "ElectionId")
                        .IsUnique();

                    b.ToTable("Votes");
                });

            modelBuilder.Entity("VotingApp.Models.Candidate", b =>
                {
                    b.HasOne("VotingApp.Models.Election", "Election")
                        .WithMany("Candidates")
                        .HasForeignKey("ElectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VotingApp.Models.User", "User")
                        .WithMany("Candidates")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Election");

                    b.Navigation("User");
                });

            modelBuilder.Entity("VotingApp.Models.UserRole", b =>
                {
                    b.HasOne("VotingApp.Models.Role", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VotingApp.Models.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("VotingApp.Models.Vote", b =>
                {
                    b.HasOne("VotingApp.Models.Candidate", "Candidate")
                        .WithMany("Votes")
                        .HasForeignKey("CandidateId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("VotingApp.Models.Election", "Election")
                        .WithMany("Votes")
                        .HasForeignKey("ElectionId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("VotingApp.Models.User", "User")
                        .WithMany("Votes")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Candidate");

                    b.Navigation("Election");

                    b.Navigation("User");
                });

            modelBuilder.Entity("VotingApp.Models.Candidate", b =>
                {
                    b.Navigation("Votes");
                });

            modelBuilder.Entity("VotingApp.Models.Election", b =>
                {
                    b.Navigation("Candidates");

                    b.Navigation("Votes");
                });

            modelBuilder.Entity("VotingApp.Models.Role", b =>
                {
                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("VotingApp.Models.User", b =>
                {
                    b.Navigation("Candidates");

                    b.Navigation("UserRoles");

                    b.Navigation("Votes");
                });
#pragma warning restore 612, 618
        }
    }
}
