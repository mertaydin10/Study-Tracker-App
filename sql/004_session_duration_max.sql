-- Süre API ile aynı: 1–1440 dakika. Mevcut volume’da elle çalıştır.

ALTER TABLE study_sessions
    DROP CONSTRAINT IF EXISTS ck_study_sessions_duration_positive;

ALTER TABLE study_sessions
    ADD CONSTRAINT ck_study_sessions_duration_positive
    CHECK (duration_minutes > 0 AND duration_minutes <= 1440);
