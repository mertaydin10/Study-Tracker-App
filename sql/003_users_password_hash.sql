-- JWT için hash; düz şifre saklanmaz.
-- Demo giriş: demo@local / demo
-- Mevcut volume: psql -h 127.0.0.1 -p 5433 -U postgres -d study_tracker -f sql/003_users_password_hash.sql

ALTER TABLE users
    ADD COLUMN IF NOT EXISTS password_hash TEXT NOT NULL DEFAULT '';

UPDATE users
SET password_hash = 'AQAAAAIAAYagAAAAEFQQFbtF5rkBKQrTA05xhRvCzAAa9XEDQZ/e6+eEF2E1O7UIY5hBVVG5cmMKj8sY+g=='
WHERE email = 'demo@local';

ALTER TABLE users
    ALTER COLUMN password_hash DROP DEFAULT;
