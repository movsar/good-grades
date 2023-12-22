﻿// <auto-generated />
using System;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Data.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("Data.Entities.CelebrityWordsQuizEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("ModifiedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("CelebrityWordsQuizes");
                });

            modelBuilder.Entity("Data.Entities.DbMetaEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("AppVersion")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("ModifiedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("DbMetas");
                });

            modelBuilder.Entity("Data.Entities.GapFillerQuizEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("ModifiedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("GapFillerQuizes");
                });

            modelBuilder.Entity("Data.Entities.ListeningMaterialEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Audio")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Image")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<DateTimeOffset>("ModifiedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("SegmentEntityId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("SegmentEntityId");

                    b.ToTable("ListeningMaterials");
                });

            modelBuilder.Entity("Data.Entities.ProverbBuilderQuizEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("ModifiedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ProverbBuilderQuizes");
                });

            modelBuilder.Entity("Data.Entities.ProverbSelectionQuizEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("CorrectQuizId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("ModifiedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ProverbSelectionQuizes");
                });

            modelBuilder.Entity("Data.Entities.QuizItemEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("CelebrityWordsQuizEntityId")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("GapFillerQuizEntityId")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Image")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<DateTimeOffset>("ModifiedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProverbBuilderQuizEntityId")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProverbSelectionQuizEntityId")
                        .HasColumnType("TEXT");

                    b.Property<string>("TestingQuestionEntityId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CelebrityWordsQuizEntityId");

                    b.HasIndex("GapFillerQuizEntityId");

                    b.HasIndex("ProverbBuilderQuizEntityId");

                    b.HasIndex("ProverbSelectionQuizEntityId");

                    b.HasIndex("TestingQuestionEntityId");

                    b.ToTable("QuizItems");
                });

            modelBuilder.Entity("Data.Entities.ReadingMaterialEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Image")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<DateTimeOffset>("ModifiedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("SegmentEntityId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("SegmentEntityId");

                    b.ToTable("ReadingMaterials");
                });

            modelBuilder.Entity("Data.Entities.SegmentEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("CelebrityWordsQuizId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("GapFillerQuizId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("ModifiedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProverbBuilderQuizId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ProverbSelectionQuizId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TestingQuizId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CelebrityWordsQuizId");

                    b.HasIndex("GapFillerQuizId");

                    b.HasIndex("ProverbBuilderQuizId");

                    b.HasIndex("ProverbSelectionQuizId");

                    b.HasIndex("TestingQuizId");

                    b.ToTable("Segments");
                });

            modelBuilder.Entity("Data.Entities.TestingQuestionEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("CorrectQuizId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("ModifiedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("QuestionText")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TestingQuizEntityId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("TestingQuizEntityId");

                    b.ToTable("TestingQuestions");
                });

            modelBuilder.Entity("Data.Entities.TestingQuizEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("ModifiedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TestingQuizItems");
                });

            modelBuilder.Entity("Data.Entities.ListeningMaterialEntity", b =>
                {
                    b.HasOne("Data.Entities.SegmentEntity", null)
                        .WithMany("ListeningMaterials")
                        .HasForeignKey("SegmentEntityId");
                });

            modelBuilder.Entity("Data.Entities.QuizItemEntity", b =>
                {
                    b.HasOne("Data.Entities.CelebrityWordsQuizEntity", null)
                        .WithMany("QuizItems")
                        .HasForeignKey("CelebrityWordsQuizEntityId");

                    b.HasOne("Data.Entities.GapFillerQuizEntity", null)
                        .WithMany("QuizItems")
                        .HasForeignKey("GapFillerQuizEntityId");

                    b.HasOne("Data.Entities.ProverbBuilderQuizEntity", null)
                        .WithMany("QuizItems")
                        .HasForeignKey("ProverbBuilderQuizEntityId");

                    b.HasOne("Data.Entities.ProverbSelectionQuizEntity", null)
                        .WithMany("QuizItems")
                        .HasForeignKey("ProverbSelectionQuizEntityId");

                    b.HasOne("Data.Entities.TestingQuestionEntity", null)
                        .WithMany("QuizItems")
                        .HasForeignKey("TestingQuestionEntityId");
                });

            modelBuilder.Entity("Data.Entities.ReadingMaterialEntity", b =>
                {
                    b.HasOne("Data.Entities.SegmentEntity", null)
                        .WithMany("ReadingMaterials")
                        .HasForeignKey("SegmentEntityId");
                });

            modelBuilder.Entity("Data.Entities.SegmentEntity", b =>
                {
                    b.HasOne("Data.Entities.CelebrityWordsQuizEntity", "CelebrityWordsQuiz")
                        .WithMany()
                        .HasForeignKey("CelebrityWordsQuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Data.Entities.GapFillerQuizEntity", "GapFillerQuiz")
                        .WithMany()
                        .HasForeignKey("GapFillerQuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Data.Entities.ProverbBuilderQuizEntity", "ProverbBuilderQuiz")
                        .WithMany()
                        .HasForeignKey("ProverbBuilderQuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Data.Entities.ProverbSelectionQuizEntity", "ProverbSelectionQuiz")
                        .WithMany()
                        .HasForeignKey("ProverbSelectionQuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Data.Entities.TestingQuizEntity", "TestingQuiz")
                        .WithMany()
                        .HasForeignKey("TestingQuizId");

                    b.Navigation("CelebrityWordsQuiz");

                    b.Navigation("GapFillerQuiz");

                    b.Navigation("ProverbBuilderQuiz");

                    b.Navigation("ProverbSelectionQuiz");

                    b.Navigation("TestingQuiz");
                });

            modelBuilder.Entity("Data.Entities.TestingQuestionEntity", b =>
                {
                    b.HasOne("Data.Entities.TestingQuizEntity", null)
                        .WithMany("Questions")
                        .HasForeignKey("TestingQuizEntityId");
                });

            modelBuilder.Entity("Data.Entities.CelebrityWordsQuizEntity", b =>
                {
                    b.Navigation("QuizItems");
                });

            modelBuilder.Entity("Data.Entities.GapFillerQuizEntity", b =>
                {
                    b.Navigation("QuizItems");
                });

            modelBuilder.Entity("Data.Entities.ProverbBuilderQuizEntity", b =>
                {
                    b.Navigation("QuizItems");
                });

            modelBuilder.Entity("Data.Entities.ProverbSelectionQuizEntity", b =>
                {
                    b.Navigation("QuizItems");
                });

            modelBuilder.Entity("Data.Entities.SegmentEntity", b =>
                {
                    b.Navigation("ListeningMaterials");

                    b.Navigation("ReadingMaterials");
                });

            modelBuilder.Entity("Data.Entities.TestingQuestionEntity", b =>
                {
                    b.Navigation("QuizItems");
                });

            modelBuilder.Entity("Data.Entities.TestingQuizEntity", b =>
                {
                    b.Navigation("Questions");
                });
#pragma warning restore 612, 618
        }
    }
}