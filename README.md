# Study Tracker

ASP.NET Core (net10) Web API + Postgres öğrenme uygulaması. Statik arayüz `wwwroot` içinde; JWT ile kullanıcı ayrılır.

Tek API projesi: `src/StudyTracker.Api`. EF Core Npgsql kullanır; HTTP’de entity yok, DTO var. IDENTITY (`BIGINT`) boşlukları doldurulmaz.

## Ne var

- Kayıt / giriş, `GET|PUT /api/auth/me`, şifre değiştirme
- Konu ve oturum CRUD, etiketler, konu/etiket/tarih filtresi, sayfalama (10/20)
- Özet `GET /api/stats/summary`, CSV `GET /api/sessions/export`
- `/health` (anonim), Swagger `/swagger` (Development)
- Test: `tests/StudyTracker.Api.Tests` (InMemory)

JWT ~8 saat; refresh token yok.

## Çalıştırma (yerel)

Host **5432** kurs Postgres / Homebrew için kalsın. Bu proje **5433**.

```bash
docker compose up -d postgres
```

Boş volume’da SQL’i sırayla çalıştır (`002` demoyu ekler, `003` şifre hash’i, `004` süre 1–1440):

```bash
export PGPASSWORD=postgres
for f in sql/001_schema.sql sql/002_seed.sql sql/003_users_password_hash.sql sql/004_session_duration_max.sql; do
  psql -h 127.0.0.1 -p 5433 -U postgres -d study_tracker -f "$f"
done
```

Mevcut volume’da `001`–`003` tekrar çalıştırma. `004` bir kez, constraint yoksa.

```bash
dotnet run --project src/StudyTracker.Api
```

- Uygulama: [http://localhost:5010](http://localhost:5010)
- Demo: `demo@local` / `demo`
- Adminer (isteğe bağlı): `docker compose up -d adminer` → [http://localhost:8080](http://localhost:8080)  
  Sistem: PostgreSQL, sunucu: `postgres`, kullanıcı/şifre: `postgres`, veritabanı: `study_tracker`

Development’ta HTTP kullanılır; HTTPS yönlendirmesi kapalı.

`dotnet ef database update` mevcut Docker volume’a uygulanmaz; `InitialCreate` orada hiç çalışmadı. Şema `sql/*.sql`.

Bağlantı: `appsettings.json` → `Host=127.0.0.1;Port=5433;Database=study_tracker;Username=postgres;Password=postgres`.

## Test

```bash
dotnet test
```

Postgres gerekmez.

## Docker’da API

```bash
docker compose up -d --build
```

API konteyneri `5010→8080`, Postgres’e Docker ağı üzerinden (`Host=postgres;Port=5432`) bağlanır. Şema yine elle `sql/`; compose `initdb` çalıştırmaz. `.dockerignore` `bin`/`obj` almaz.

## API özeti

| | |
| --- | --- |
| `GET /health` | DB durumu |
| `POST /api/auth/login` `register` | JWT |
| `GET` `PUT /api/auth/me` | profil |
| `POST /api/auth/change-password` | şifre |
| `/api/subjects` `/api/tags` `/api/sessions` | CRUD (+ oturum `export`) |
| `GET /api/stats/summary` | `from` `to` `subjectId` |

Oturum listesi: `from` `to` `subjectId` `tagId` `page` `pageSize` (en fazla 50).

## Kapsam dışı

Clean Architecture, MediatR, e-posta ile şifre sıfırlama yok.
