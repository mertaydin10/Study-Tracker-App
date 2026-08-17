using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StudyTracker.Api.Data;

#nullable disable

namespace StudyTracker.Api.Data.Migrations
{
    [DbContext(typeof(StudyTrackerDbContext))]
    partial class StudyTrackerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("StudyTracker.Api.Entities.StudySession", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long>("Id"));

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<int>("DurationMinutes")
                        .HasColumnType("integer")
                        .HasColumnName("duration_minutes");

                    b.Property<string>("Notes")
                        .HasColumnType("text")
                        .HasColumnName("notes");

                    b.Property<DateTimeOffset>("StartedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("started_at");

                    b.Property<long>("SubjectId")
                        .HasColumnType("bigint")
                        .HasColumnName("subject_id");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("SubjectId")
                        .HasDatabaseName("ix_study_sessions_subject");

                    b.HasIndex("UserId", "StartedAt")
                        .IsDescending(false, true)
                        .HasDatabaseName("ix_study_sessions_user_started");

                    b.ToTable("study_sessions", null, t =>
                        {
                            t.HasCheckConstraint("ck_study_sessions_duration_positive", "duration_minutes > 0");
                        });
                });

            modelBuilder.Entity("StudyTracker.Api.Entities.StudySessionTag", b =>
                {
                    b.Property<long>("StudySessionId")
                        .HasColumnType("bigint")
                        .HasColumnName("study_session_id");

                    b.Property<long>("TagId")
                        .HasColumnType("bigint")
                        .HasColumnName("tag_id");

                    b.HasKey("StudySessionId", "TagId")
                        .HasName("pk_study_session_tags");

                    b.HasIndex("TagId")
                        .HasDatabaseName("ix_study_session_tags_tag");

                    b.ToTable("study_session_tags", (string)null);
                });

            modelBuilder.Entity("StudyTracker.Api.Entities.Subject", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long>("Id"));

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "Name")
                        .IsUnique()
                        .HasDatabaseName("uq_subjects_user_name");

                    b.ToTable("subjects", (string)null);
                });

            modelBuilder.Entity("StudyTracker.Api.Entities.Tag", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "Name")
                        .IsUnique()
                        .HasDatabaseName("uq_tags_user_name");

                    b.ToTable("tags", (string)null);
                });

            modelBuilder.Entity("StudyTracker.Api.Entities.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long>("Id"));

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("display_name");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasDatabaseName("uq_users_email");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("StudyTracker.Api.Entities.StudySession", b =>
                {
                    b.HasOne("StudyTracker.Api.Entities.Subject", "Subject")
                        .WithMany("StudySessions")
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("StudyTracker.Api.Entities.User", "User")
                        .WithMany("StudySessions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Subject");

                    b.Navigation("User");
                });

            modelBuilder.Entity("StudyTracker.Api.Entities.StudySessionTag", b =>
                {
                    b.HasOne("StudyTracker.Api.Entities.StudySession", "StudySession")
                        .WithMany("SessionTags")
                        .HasForeignKey("StudySessionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StudyTracker.Api.Entities.Tag", "Tag")
                        .WithMany("SessionTags")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("StudySession");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("StudyTracker.Api.Entities.Subject", b =>
                {
                    b.HasOne("StudyTracker.Api.Entities.User", "User")
                        .WithMany("Subjects")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("StudyTracker.Api.Entities.Tag", b =>
                {
                    b.HasOne("StudyTracker.Api.Entities.User", "User")
                        .WithMany("Tags")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("StudyTracker.Api.Entities.StudySession", b =>
                {
                    b.Navigation("SessionTags");
                });

            modelBuilder.Entity("StudyTracker.Api.Entities.Subject", b =>
                {
                    b.Navigation("StudySessions");
                });

            modelBuilder.Entity("StudyTracker.Api.Entities.Tag", b =>
                {
                    b.Navigation("SessionTags");
                });

            modelBuilder.Entity("StudyTracker.Api.Entities.User", b =>
                {
                    b.Navigation("StudySessions");

                    b.Navigation("Subjects");

                    b.Navigation("Tags");
                });
#pragma warning restore 612, 618
        }
    }
}
