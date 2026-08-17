-- Study Tracker — şema v1 (API yok; elle çalıştır).
-- BIGINT IDENTITY: C# long ile birebir, UUID'den okunması kolay.
-- TIMESTAMPTZ: saat dilimi kaybı olmasın (TIMESTAMP local'e bağlı kalır).

CREATE TABLE users (
    id              BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    email           TEXT        NOT NULL,
    display_name    TEXT        NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT uq_users_email UNIQUE (email)
);

-- Konu kullanıcıya ait. Aynı isim iki kullanıcıda olabilir; bir kullanıcıda tekrar edemez.
CREATE TABLE subjects (
    id          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id     BIGINT      NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    name        TEXT        NOT NULL,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT uq_subjects_user_name UNIQUE (user_id, name)
);

CREATE TABLE study_sessions (
    id                  BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id             BIGINT      NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    subject_id          BIGINT      NOT NULL REFERENCES subjects (id) ON DELETE RESTRICT,
    started_at          TIMESTAMPTZ NOT NULL,
    duration_minutes    INTEGER     NOT NULL,
    notes               TEXT        NULL,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT ck_study_sessions_duration_positive CHECK (duration_minutes > 0)
);

-- subject ON DELETE RESTRICT: oturumu olan konuyu silmek hata verir.
-- Oturum silinince konu kalsın; konu silinince geçmiş istatistik kaybolmasın.

CREATE TABLE tags (
    id          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id     BIGINT  NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    name        TEXT    NOT NULL,
    CONSTRAINT uq_tags_user_name UNIQUE (user_id, name)
);

CREATE TABLE study_session_tags (
    study_session_id    BIGINT NOT NULL REFERENCES study_sessions (id) ON DELETE CASCADE,
    tag_id              BIGINT NOT NULL REFERENCES tags (id) ON DELETE CASCADE,
    CONSTRAINT pk_study_session_tags PRIMARY KEY (study_session_id, tag_id)
);

-- Liste/filtre: user + tarih aralığı. N+1'den bağımsız; SQL tarafında index.
CREATE INDEX ix_study_sessions_user_started
    ON study_sessions (user_id, started_at DESC);

CREATE INDEX ix_study_sessions_subject
    ON study_sessions (subject_id);

CREATE INDEX ix_study_session_tags_tag
    ON study_session_tags (tag_id);
