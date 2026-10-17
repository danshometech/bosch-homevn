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

Trên server tách 2 thư mục:

```
~/boschhome-vn/        source code — xóa rồi chép bản mới mỗi lần deploy
~/boschhome-rclone/    cấu hình riêng của server — KHÔNG xóa
  .env                 biến môi trường của docker compose (mật khẩu DB, admin, Turnstile…)
  rclone.conf          đăng nhập Google Drive cho backup
  backup.sh            bản chép của deploy/backup.sh (cron chạy bản này)
  backup.log
```

Lần đầu: `mkdir -p ~/boschhome-rclone && cp ~/boschhome-vn/.env.example ~/boschhome-rclone/.env` rồi điền,
`chmod 600 ~/boschhome-rclone/.env`. Mỗi lần deploy (sau khi chép source mới vào `~/boschhome-vn`):

```bash
ln -sf ~/boschhome-rclone/.env ~/boschhome-vn/.env
cd ~/boschhome-vn && docker compose up -d --build
```

Database và ảnh / video nằm trong PostgreSQL và volume docker `bosch-homevn_appdata`, xóa thư mục source không ảnh hưởng.

## Backup

`backup.sh` chép lên Google Drive, thư mục `bosch-homevn-backup`:

- `db/` — dump database mỗi ngày (`boschhomevn-YYYY-MM-DD.dump`), giữ 30 ngày
- `appdata/` — ảnh / video, khóa đăng nhập; chỉ tải file mới, file xóa trên web vẫn còn trên Drive
- `env/.env` — có mật khẩu, nên bật xác minh 2 bước cho tài khoản Google

Script đọc `.env` và `rclone.conf` trong `~/boschhome-rclone` (đổi bằng biến `BOSCHHOME_CONFIG`), không phụ thuộc
thư mục source. Chép script sang đó một lần (và chép lại khi `deploy/backup.sh` có thay đổi):
`cp ~/boschhome-vn/deploy/backup.sh ~/boschhome-rclone/`.

1. Đăng nhập Google Drive (server không có trình duyệt nên mượn máy tính của bạn):
   - Trên server: `bash ~/boschhome-rclone/backup.sh config` → `n` (new remote) → name `gdrive` → Storage `drive` →
     client_id / client_secret bỏ trống → scope `drive.file` → service_account_file bỏ trống → advanced config `n` →
     *Use web browser* **`n`** → rclone in ra lệnh `rclone authorize "drive" "..."`.
   - Trên máy Windows: `winget install Rclone.Rclone`, mở terminal mới chạy đúng lệnh đó, đăng nhập Google → chép
     token in ra, dán lại vào server → Shared Drive `n` → `y` → `q`.
   - Đã có sẵn `rclone.conf` thì chép vào `~/boschhome-rclone/rclone.conf` thay cho bước này.
2. Chạy thử `bash ~/boschhome-rclone/backup.sh`, mở drive.google.com xem thư mục `bosch-homevn-backup`.
3. Đặt lịch (`crontab -e`, user có quyền docker). Giờ theo múi giờ server — server UTC thì `0 20 * * *` = 3h sáng VN:
   ```
   0 20 * * * bash $HOME/boschhome-rclone/backup.sh >> $HOME/boschhome-rclone/backup.log 2>&1
   ```
4. Nên có: tạo check trên healthchecks.io (Period 1 day, Grace 2 hours), dán URL ping vào `BACKUP_PING_URL` — có
   email báo khi backup lỗi hoặc cron không chạy.

### Khôi phục (server mới)

```bash
# Chép source lên ~/boschhome-vn, chép script sang thư mục cấu hình, đăng nhập Drive (bước 1 ở trên)
mkdir -p ~/boschhome-rclone && cp ~/boschhome-vn/deploy/backup.sh ~/boschhome-rclone/
# Tải thư mục backup về ~/boschhome-rclone/restore
# (hoặc tải từ drive.google.com rồi upload lên server, cùng cấu trúc thư mục)
bash ~/boschhome-rclone/backup.sh copy gdrive:bosch-homevn-backup /restore
cd ~/boschhome-rclone
cp restore/env/.env .env && chmod 600 .env    # sửa DB_NETWORK / DB_CONNECTION nếu PostgreSQL mới khác
ln -sf ~/boschhome-rclone/.env ~/boschhome-vn/.env

# Database: tạo user + DB trống rồi nạp bản mới nhất — làm TRƯỚC khi chạy compose (Api khởi động sẽ tự tạo DB mới)
docker exec <container-postgres> psql -U postgres \
  -c "CREATE USER boschhomevn PASSWORD '<mật khẩu>'" -c "CREATE DATABASE boschhomevn OWNER boschhomevn"
docker exec -i <container-postgres> pg_restore -U boschhomevn -d boschhomevn --no-owner --exit-on-error \
  < "$(ls ~/boschhome-rclone/restore/db/*.dump | tail -n1)"

# Ảnh / video: tạo volume (chưa chạy container), chép vào, trả quyền cho user của app (1654), rồi chạy
cd ~/boschhome-vn
docker compose up --no-start --build
docker run --rm -v bosch-homevn_appdata:/data -v "$HOME/boschhome-rclone/restore/appdata:/src:ro" alpine:3.20 \
  sh -c 'cp -a /src/. /data/ && chown -R 1654:1654 /data'
docker compose up -d
```
