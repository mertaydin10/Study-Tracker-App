# Study Tracker

ASP.NET Core Web API + Postgres öğrenme projesi. Konu ve çalışma oturumu tutulur; JWT ile kullanıcı ayrılır.

## Çalıştırma

1. Postgres: `docker compose up -d postgres` (host **5433**, kurs/Homebrew 5432’yi kullanır).
2. Şema (boş volume’da bir kez): `sql/001_schema.sql` … `004_session_duration_max.sql`.
3. API: `dotnet run --project src/StudyTracker.Api` → [http://localhost:5010](http://localhost:5010)
4. Demo: `demo@local` / `demo`. Swagger: `/swagger`. Durum: `/health`.
5. Test: `dotnet test`

Development’ta HTTP kullanılır; HTTPS yönlendirmesi yok. Docker imajına `bin`/`obj` girmez (`.dockerignore`).

`dotnet ef database update` mevcut Docker volume’a uygulanmaz; InitialCreate orada hiç çalışmadı. `004` mevcut DB’de elle çalışır.

## Kapsam dışı

Clean Architecture / MediatR yok. IDENTITY boşlukları doldurulmaz. Refresh token yok (JWT 8 saat).
