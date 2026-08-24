using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Api.Dtos.Sessions;

public sealed class CreateSessionRequest
{
    [Range(1, long.MaxValue, ErrorMessage = "Konu seç.")]
    public long SubjectId { get; set; }

    [Required(ErrorMessage = "Başlangıç gerekli.")]
    public DateTimeOffset StartedAt { get; set; }

    [Range(1, 1440, ErrorMessage = "Dakika 1 ile 1440 arasında olmalı.")]
    public int DurationMinutes { get; set; }

    [MaxLength(2000, ErrorMessage = "Not en fazla 2000 karakter olabilir.")]
    public string? Notes { get; set; }

    public List<long> TagIds { get; set; } = [];
}
