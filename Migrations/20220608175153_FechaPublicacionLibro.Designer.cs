﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WebApiAutores;

#nullable disable

namespace WebApiAutores.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20220608175153_FechaPublicacionLibro")]
    partial class FechaPublicacionLibro
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-preview.4.22229.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WebApiAutores.Entidades.Autor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("character varying(12)");

                    b.HasKey("Id");

                    b.ToTable("Autores");
                });

            modelBuilder.Entity("WebApiAutores.Entidades.AutorLibro", b =>
                {
                    b.Property<int>("AutorId")
                        .HasColumnType("integer");

                    b.Property<int>("LibroId")
                        .HasColumnType("integer");

                    b.Property<int>("Orden")
                        .HasColumnType("integer");

                    b.HasKey("AutorId", "LibroId");

                    b.HasIndex("LibroId");

                    b.ToTable("AutoresLibros");
                });

            modelBuilder.Entity("WebApiAutores.Entidades.Comentario", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Contenido")
                        .HasColumnType("text");

                    b.Property<int>("LibroId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("LibroId");

                    b.ToTable("Comentarios");
                });

            modelBuilder.Entity("WebApiAutores.Entidades.Libro", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("FechaPublicacion")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Titulo")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Libros");
                });

            modelBuilder.Entity("WebApiAutores.Entidades.AutorLibro", b =>
                {
                    b.HasOne("WebApiAutores.Entidades.Autor", "Autor")
                        .WithMany("AutoresLibros")
                        .HasForeignKey("AutorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebApiAutores.Entidades.Libro", "Libro")
                        .WithMany("AutoresLibros")
                        .HasForeignKey("LibroId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Autor");

                    b.Navigation("Libro");
                });

            modelBuilder.Entity("WebApiAutores.Entidades.Comentario", b =>
                {
                    b.HasOne("WebApiAutores.Entidades.Libro", "Libro")
                        .WithMany("Comentarios")
                        .HasForeignKey("LibroId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Libro");
                });

            modelBuilder.Entity("WebApiAutores.Entidades.Autor", b =>
                {
                    b.Navigation("AutoresLibros");
                });

            modelBuilder.Entity("WebApiAutores.Entidades.Libro", b =>
                {
                    b.Navigation("AutoresLibros");

                    b.Navigation("Comentarios");
                });
#pragma warning restore 612, 618
        }
    }
}
