﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyMvcApp.Data.Localize;

#nullable disable

namespace MyMvcApp.Data.Localize.Migrations
{
    [DbContext(typeof(LocalizeDbContext))]
    [Migration("20220922121859_CreateLocalizeSchema")]
    partial class CreateLocalizeSchema
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("MyMvcApp.Data.Localize.Domain", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Cultures")
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("Id");

                    b.ToTable("Domain", "localize");
                });

            modelBuilder.Entity("MyMvcApp.Data.Localize.Key", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ArgumentNames")
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<int>("DomainId")
                        .HasColumnType("int");

                    b.Property<string>("ForPath")
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ValuesToReview")
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.HasKey("Id");

                    b.HasIndex("DomainId");

                    b.ToTable("Key", "localize");
                });

            modelBuilder.Entity("MyMvcApp.Data.Localize.KeyValue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Culture")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<int>("KeyId")
                        .HasColumnType("int");

                    b.Property<bool>("Reviewed")
                        .HasColumnType("bit");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("KeyId");

                    b.ToTable("KeyValue", "localize");
                });

            modelBuilder.Entity("MyMvcApp.Data.Localize.Query", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ConnectionName")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<int>("DomainId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("Sql")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("DomainId");

                    b.ToTable("Query", "localize");
                });

            modelBuilder.Entity("MyMvcApp.Data.Localize.Key", b =>
                {
                    b.HasOne("MyMvcApp.Data.Localize.Domain", "Domain")
                        .WithMany("Keys")
                        .HasForeignKey("DomainId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Domain");
                });

            modelBuilder.Entity("MyMvcApp.Data.Localize.KeyValue", b =>
                {
                    b.HasOne("MyMvcApp.Data.Localize.Key", "Key")
                        .WithMany("Values")
                        .HasForeignKey("KeyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Key");
                });

            modelBuilder.Entity("MyMvcApp.Data.Localize.Query", b =>
                {
                    b.HasOne("MyMvcApp.Data.Localize.Domain", "Domain")
                        .WithMany("Queries")
                        .HasForeignKey("DomainId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Domain");
                });

            modelBuilder.Entity("MyMvcApp.Data.Localize.Domain", b =>
                {
                    b.Navigation("Keys");

                    b.Navigation("Queries");
                });

            modelBuilder.Entity("MyMvcApp.Data.Localize.Key", b =>
                {
                    b.Navigation("Values");
                });
#pragma warning restore 612, 618
        }
    }
}
