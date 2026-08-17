using Microsoft.EntityFrameworkCore;
using StudyTracker.Api.Entities;

namespace StudyTracker.Api.Data;

public sealed class StudyTrackerDbContext(DbContextOptions<StudyTrackerDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<StudySession> StudySessions => Set<StudySession>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<StudySessionTag> StudySessionTags => Set<StudySessionTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Email).HasColumnName("email").IsRequired();
            entity.Property(e => e.DisplayName).HasColumnName("display_name").IsRequired();
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("uq_users_email");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("subjects");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");
            entity.HasIndex(e => new { e.UserId, e.Name })
                .IsUnique()
                .HasDatabaseName("uq_subjects_user_name");
            entity.HasOne(e => e.User)
                .WithMany(u => u.Subjects)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StudySession>(entity =>
        {
            entity.ToTable("study_sessions", table =>
            {
                table.HasCheckConstraint(
                    "ck_study_sessions_duration_positive",
                    "duration_minutes > 0");
            });
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()");
            entity.HasIndex(e => new { e.UserId, e.StartedAt })
                .IsDescending(false, true)
                .HasDatabaseName("ix_study_sessions_user_started");
            entity.HasIndex(e => e.SubjectId).HasDatabaseName("ix_study_sessions_subject");
            entity.HasOne(e => e.User)
                .WithMany(u => u.StudySessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Subject)
                .WithMany(s => s.StudySessions)
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.HasIndex(e => new { e.UserId, e.Name })
                .IsUnique()
                .HasDatabaseName("uq_tags_user_name");
            entity.HasOne(e => e.User)
                .WithMany(u => u.Tags)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StudySessionTag>(entity =>
        {
            entity.ToTable("study_session_tags");
            entity.HasKey(e => new { e.StudySessionId, e.TagId })
                .HasName("pk_study_session_tags");
            entity.Property(e => e.StudySessionId).HasColumnName("study_session_id");
            entity.Property(e => e.TagId).HasColumnName("tag_id");
            entity.HasIndex(e => e.TagId).HasDatabaseName("ix_study_session_tags_tag");
            entity.HasOne(e => e.StudySession)
                .WithMany(s => s.SessionTags)
                .HasForeignKey(e => e.StudySessionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Tag)
                .WithMany(t => t.SessionTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
