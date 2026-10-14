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

## Backup

`deploy/backup.sh` chép lên Google Drive, thư mục `bosch-homevn-backup`:

- `db/` — dump database mỗi ngày (`boschhomevn-YYYY-MM-DD.dump`), giữ 30 ngày
- `appdata/` — ảnh / video, khóa đăng nhập; chỉ tải file mới, file xóa trên web vẫn còn trên Drive
- `env/.env` — có mật khẩu, nên bật xác minh 2 bước cho tài khoản Google

1. Đăng nhập Google Drive (server không có trình duyệt nên mượn máy tính của bạn):
   - Trên server: `bash deploy/backup.sh config` → `n` (new remote) → name `gdrive` → Storage `drive` →
     client_id / client_secret bỏ trống → scope `drive.file` → service_account_file bỏ trống → advanced config `n` →
     *Use web browser* **`n`** → rclone in ra lệnh `rclone authorize "drive" "..."`.
   - Trên máy Windows: `winget install Rclone.Rclone`, mở terminal mới chạy đúng lệnh đó, đăng nhập Google → chép
     token in ra, dán lại vào server → Shared Drive `n` → `y` → `q`.
2. Chạy thử `bash deploy/backup.sh`, mở drive.google.com xem thư mục `bosch-homevn-backup`.
3. Đặt lịch (`crontab -e`, user có quyền docker). Giờ theo múi giờ server — server UTC thì `0 20 * * *` = 3h sáng VN:
   ```
   0 20 * * * bash /đường/dẫn/bosch-homevn/deploy/backup.sh >> /var/log/bosch-homevn-backup.log 2>&1
   ```
4. Nên có: tạo check trên healthchecks.io (Period 1 day, Grace 2 hours), dán URL ping vào `BACKUP_PING_URL` — có
   email báo khi backup lỗi hoặc cron không chạy.

### Khôi phục (server mới)

```bash
# Chép source lên, đăng nhập Drive (bước 1 ở trên), tải thư mục backup về ./restore
# (hoặc tải từ drive.google.com rồi upload lên server, cùng cấu trúc thư mục)
bash deploy/backup.sh copy gdrive:bosch-homevn-backup /restore
cp restore/env/.env .env      # sửa DB_NETWORK / DB_CONNECTION nếu PostgreSQL mới khác

# Database: tạo user + DB trống rồi nạp bản mới nhất — làm TRƯỚC khi chạy compose (Api khởi động sẽ tự tạo DB mới)
docker exec <container-postgres> psql -U postgres \
  -c "CREATE USER boschhomevn PASSWORD '<mật khẩu>'" -c "CREATE DATABASE boschhomevn OWNER boschhomevn"
docker exec -i <container-postgres> pg_restore -U boschhomevn -d boschhomevn --no-owner --exit-on-error \
  < "$(ls restore/db/*.dump | tail -n1)"

# Ảnh / video: tạo volume (chưa chạy container), chép vào, trả quyền cho user của app (1654), rồi chạy
docker compose up --no-start --build
docker run --rm -v bosch-homevn_appdata:/data -v "$PWD/restore/appdata:/src:ro" alpine:3.20 \
  sh -c 'cp -a /src/. /data/ && chown -R 1654:1654 /data'
docker compose up -d
```
