﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyLoveAgency.Models.Database;

#nullable disable

namespace MyLoveAgency.Models.Migrations
{
    [DbContext(typeof(LovelyLoveDbContext))]
    partial class LovelyLoveDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.15")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("MyLoveAgency.Models.Database.Contact", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Communication")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("communication")
                        .HasDefaultValueSql("(N'Не указан')");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime")
                        .HasColumnName("date");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("email");

                    b.Property<int?>("IdPackage")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id_package")
                        .HasDefaultValueSql("((0))");

                    b.Property<int>("IdService")
                        .HasColumnType("int")
                        .HasColumnName("id_service");

                    b.Property<string>("Message")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)")
                        .HasColumnName("message");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("name");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("phone_number");

                    b.HasKey("Id");

                    b.ToTable("Contact", (string)null);
                });

            modelBuilder.Entity("MyLoveAgency.Models.Database.Faq", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AnswerEn")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("answer_en");

                    b.Property<string>("AnswerUa")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("answer_ua");

                    b.Property<string>("QuestionEn")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("question_en");

                    b.Property<string>("QuestionUa")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("question_ua");

                    b.HasKey("Id");

                    b.ToTable("FAQ", (string)null);
                });

            modelBuilder.Entity("MyLoveAgency.Models.Database.Localization", b =>
                {
                    b.Property<string>("En")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("en");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("name");

                    b.Property<string>("Pl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Ua")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("ua");

                    b.ToTable("Localization", (string)null);
                });

            modelBuilder.Entity("MyLoveAgency.Models.Database.PackageService", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DescriptionEn")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("description_en");

                    b.Property<string>("DescriptionUa")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("description_ua");

                    b.Property<string>("DurationEn")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("duration_en");

                    b.Property<string>("DurationUa")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("duration_ua");

                    b.Property<int>("IdService")
                        .HasColumnType("int")
                        .HasColumnName("id_service");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("name");

                    b.Property<string>("PriceZloty")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("price_zloty");

                    b.HasKey("Id");

                    b.HasIndex("IdService");

                    b.ToTable("PackageService", (string)null);
                });

            modelBuilder.Entity("MyLoveAgency.Models.Database.Service", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DescriptionEn")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("description_en");

                    b.Property<string>("DescriptionUa")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("description_ua");

                    b.Property<int>("IdType")
                        .HasColumnType("int")
                        .HasColumnName("id_type");

                    b.Property<string>("NameEn")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("name_en");

                    b.Property<string>("NameUa")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("name_ua");

                    b.Property<string>("PriceZloty")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("price_zloty")
                        .HasDefaultValueSql("((0))");

                    b.HasKey("Id");

                    b.HasIndex("IdType");

                    b.ToTable("Service", (string)null);
                });

            modelBuilder.Entity("MyLoveAgency.Models.Database.StorageImageGallery", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Number")
                        .HasColumnType("int")
                        .HasColumnName("number");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("path");

                    b.HasKey("Id");

                    b.ToTable("StorageImageGallery", (string)null);
                });

            modelBuilder.Entity("MyLoveAgency.Models.Database.StorageImageService", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("IdService")
                        .HasColumnType("int")
                        .HasColumnName("id_service");

                    b.Property<int>("Number")
                        .HasColumnType("int")
                        .HasColumnName("number");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("path");

                    b.HasKey("Id");

                    b.HasIndex("IdService");

                    b.ToTable("StorageImageService", (string)null);
                });

            modelBuilder.Entity("MyLoveAgency.Models.Database.TypeService", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DescriptionEn")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("description_en");

                    b.Property<string>("DescriptionUa")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("description_ua");

                    b.Property<string>("NameEn")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("name_en");

                    b.Property<string>("NameUa")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("name_ua");

                    b.HasKey("Id")
                        .HasName("PK_Type");

                    b.ToTable("TypeService", (string)null);
                });

            modelBuilder.Entity("MyLoveAgency.Models.Database.PackageService", b =>
                {
                    b.HasOne("MyLoveAgency.Models.Database.Service", "IdServiceNavigation")
                        .WithMany("PackageServices")
                        .HasForeignKey("IdService")
                        .IsRequired()
                        .HasConstraintName("FK_PackageService_Service");

                    b.Navigation("IdServiceNavigation");
                });

            modelBuilder.Entity("MyLoveAgency.Models.Database.Service", b =>
                {
                    b.HasOne("MyLoveAgency.Models.Database.TypeService", "IdTypeNavigation")
                        .WithMany("Services")
                        .HasForeignKey("IdType")
                        .IsRequired()
                        .HasConstraintName("FK_Service_TypeService");

                    b.Navigation("IdTypeNavigation");
                });

            modelBuilder.Entity("MyLoveAgency.Models.Database.StorageImageService", b =>
                {
                    b.HasOne("MyLoveAgency.Models.Database.Service", "IdServiceNavigation")
                        .WithMany("StorageImageServices")
                        .HasForeignKey("IdService")
                        .IsRequired()
                        .HasConstraintName("FK_StorageImageService_Service");

                    b.Navigation("IdServiceNavigation");
                });

            modelBuilder.Entity("MyLoveAgency.Models.Database.Service", b =>
                {
                    b.Navigation("PackageServices");

                    b.Navigation("StorageImageServices");
                });

            modelBuilder.Entity("MyLoveAgency.Models.Database.TypeService", b =>
                {
                    b.Navigation("Services");
                });
#pragma warning restore 612, 618
        }
    }
}
