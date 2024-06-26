﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductsService.Data;

#nullable disable

namespace ProductsService.Migrations
{
    [DbContext(typeof(ProductContext))]
    partial class ProductContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ProductsService.Models.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Cost")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<double>("StockQuantity")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.ToTable("Products");

                    b.HasData(
                        new
                        {
                            Id = new Guid("a0fa5a1f-fc38-4491-90da-2b04ea7bd679"),
                            Cost = 5m,
                            Description = "Tasty beverage to kill your thirst",
                            Name = "Nuka-Cola",
                            StockQuantity = 10.0
                        },
                        new
                        {
                            Id = new Guid("0f5583e2-d5a3-491b-8e13-f57e04f46083"),
                            Cost = 25m,
                            Description = "A pie from the past",
                            Name = "Perfectly Preserved Pie",
                            StockQuantity = 1.0
                        },
                        new
                        {
                            Id = new Guid("7223634c-c992-4967-9cda-4cb4192f0a2e"),
                            Cost = 10m,
                            Description = "For adding lube whenever you need",
                            Name = "Aluminum Oil Can",
                            StockQuantity = 100.0
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
