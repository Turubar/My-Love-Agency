using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MyLoveAgency.Models.Database;

public partial class LovelyLoveDbContext : DbContext
{
    public LovelyLoveDbContext()
    {
    }

    public LovelyLoveDbContext(DbContextOptions<LovelyLoveDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<Faq> Faqs { get; set; }

    public virtual DbSet<Localization> Localizations { get; set; }

    public virtual DbSet<PackageService> PackageServices { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<StorageImageGallery> StorageImageGalleries { get; set; }

    public virtual DbSet<StorageImageService> StorageImageServices { get; set; }

    public virtual DbSet<TypeService> TypeServices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlServer(DataClass.connectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("Contact");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Communication)
                .HasMaxLength(50)
                .HasDefaultValueSql("(N'Не указан')")
                .HasColumnName("communication");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.IdPackage)
                .HasDefaultValueSql("((0))")
                .HasColumnName("id_package");
            entity.Property(e => e.IdService).HasColumnName("id_service");
            entity.Property(e => e.Message)
                .HasMaxLength(500)
                .HasColumnName("message");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.ToTable("FAQ");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AnswerEn).HasColumnName("answer_en");
            entity.Property(e => e.AnswerUa).HasColumnName("answer_ua");
            entity.Property(e => e.QuestionEn).HasColumnName("question_en");
            entity.Property(e => e.QuestionUa).HasColumnName("question_ua");
        });

        modelBuilder.Entity<Localization>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Localization");

            entity.Property(e => e.En).HasColumnName("en");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Pl).HasColumnName("pl");
            entity.Property(e => e.Ua).HasColumnName("ua");
        });

        modelBuilder.Entity<PackageService>(entity =>
        {
            entity.ToTable("PackageService");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.DescriptionUa).HasColumnName("description_ua");
            entity.Property(e => e.DurationEn)
                .HasMaxLength(50)
                .HasColumnName("duration_en");
            entity.Property(e => e.DurationUa)
                .HasMaxLength(50)
                .HasColumnName("duration_ua");
            entity.Property(e => e.IdService).HasColumnName("id_service");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.PriceZloty)
                .HasMaxLength(50)
                .HasColumnName("price_zloty");

            entity.HasOne(d => d.IdServiceNavigation).WithMany(p => p.PackageServices)
                .HasForeignKey(d => d.IdService)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PackageService_Service");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("Service");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.DescriptionUa).HasColumnName("description_ua");
            entity.Property(e => e.IdType).HasColumnName("id_type");
            entity.Property(e => e.NameEn)
                .HasMaxLength(100)
                .HasColumnName("name_en");
            entity.Property(e => e.NameUa)
                .HasMaxLength(100)
                .HasColumnName("name_ua");
            entity.Property(e => e.PriceZloty)
                .HasMaxLength(50)
                .HasDefaultValueSql("((0))")
                .HasColumnName("price_zloty");

            entity.HasOne(d => d.IdTypeNavigation).WithMany(p => p.Services)
                .HasForeignKey(d => d.IdType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Service_TypeService");
        });

        modelBuilder.Entity<StorageImageGallery>(entity =>
        {
            entity.ToTable("StorageImageGallery");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.Path)
                .HasMaxLength(100)
                .HasColumnName("path");
        });

        modelBuilder.Entity<StorageImageService>(entity =>
        {
            entity.ToTable("StorageImageService");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdService).HasColumnName("id_service");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.Path)
                .HasMaxLength(100)
                .HasColumnName("path");

            entity.HasOne(d => d.IdServiceNavigation).WithMany(p => p.StorageImageServices)
                .HasForeignKey(d => d.IdService)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StorageImageService_Service");
        });

        modelBuilder.Entity<TypeService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Type");

            entity.ToTable("TypeService");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.DescriptionPl).HasColumnName("description_pl");
            entity.Property(e => e.DescriptionUa).HasColumnName("description_ua");
            entity.Property(e => e.NameEn)
                .HasMaxLength(50)
                .HasColumnName("name_en");
            entity.Property(e => e.NamePl)
                .HasMaxLength(50)
                .HasColumnName("name_pl");
            entity.Property(e => e.NameUa)
                .HasMaxLength(50)
                .HasColumnName("name_ua");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
