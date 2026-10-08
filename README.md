# bosch-homevn

Website bán hàng đại lý Bosch — .NET 10 + SQL Server, frontend Vue 3.

- `src/` — Api, WebStore (site bán hàng), Admin (quản trị) và các layer Application / Domain / Infrastructure / Contracts
- `web/` — Vue: `webstore`, `admin`

## Chạy local

```bash
cd web && npm install && npm run build && cd ..
dotnet tool restore
dotnet ef database update -p src/BoschHomeVn.Infrastructure -s src/BoschHomeVn.Api
dotnet user-secrets set "Admin:Password" "<mật khẩu>" --project src/BoschHomeVn.Api
```

Sau đó chạy 3 project Api, WebStore, Admin.

## Triển khai

```bash
cp .env.example .env
docker compose up -d --build
```
