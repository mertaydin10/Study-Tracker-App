# Study Tracker

ASP.NET Core Web API + Postgres öğrenme projesi. Konu ve çalışma oturumu tutulur; JWT ile kullanıcı ayrılır.

## Çalıştırma

1. Postgres: `docker compose up -d postgres` (host **5433**, kurs/Homebrew 5432’yi kullanır).
2. Şema (boş volume’da bir kez): `sql/001_schema.sql`, `002_seed.sql`, `003_users_password_hash.sql`.
3. API: `dotnet run --project src/StudyTracker.Api` → [http://localhost:5010](http://localhost:5010)
4. Demo: `demo@local` / `demo`. Swagger: `/swagger`.

`dotnet ef database update` mevcut Docker volume’a uygulanmaz; InitialCreate orada hiç çalışmadı.

## Kapsam dışı

Etiketler API’de durur, arayüzde yok. Clean Architecture / MediatR yok. IDENTITY boşlukları doldurulmaz.
