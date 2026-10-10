# bosch-homevn

Website bán hàng đại lý Bosch — .NET 10 + PostgreSQL, frontend Vue 3.

- `src/` — Api, WebStore (site bán hàng), Admin (quản trị) và các layer Application / Domain / Infrastructure / Contracts
- `web/` — Vue: `webstore`, `admin`

## Chạy local

```bash
cd web && npm install && npm run build && cd ..
dotnet tool restore
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Database=boschhomevn;Username=postgres;Password=<mật khẩu>" --project src/BoschHomeVn.Api
dotnet user-secrets set "Admin:Password" "<mật khẩu>" --project src/BoschHomeVn.Api
```

Sau đó chạy 3 project Api, WebStore, Admin. Api (môi trường Development) tự áp migration còn thiếu khi khởi động
(`Database:MigrateOnStartup` trong `appsettings.Development.json`).

## Triển khai

```bash
cp .env.example .env
docker compose up -d --build
```
