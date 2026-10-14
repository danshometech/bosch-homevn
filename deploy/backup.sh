#!/usr/bin/env bash
# Backup database + ảnh / video + .env lên Google Drive, thư mục bosch-homevn-backup.
#   bash deploy/backup.sh                 → backup (dump DB giữ 30 ngày; ảnh / video chỉ tải file mới)
#   bash deploy/backup.sh <lệnh rclone>   → vd. config | copy gdrive:bosch-homevn-backup /restore (ra thư mục ./restore)
set -euo pipefail

root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
env_file="$root/.env"
dest=gdrive:bosch-homevn-backup

# Đọc 1 biến trong .env theo quy tắc của docker compose ($$ = $)
env_get() {
  local line
  line=$(grep -E "^$1=" "$env_file" | tail -n1 || true)
  line=${line#*=}
  line=${line%$'\r'}
  if [[ $line =~ ^\"(.*)\"$ || $line =~ ^\'(.*)\'$ ]]; then line=${BASH_REMATCH[1]}; fi
  printf '%s' "${line//\$\$/\$}"
}

log() { echo "[$(date '+%F %T')] $*"; }

opts=()
rclone() {
  docker run --rm -e RCLONE_DRIVE_USE_TRASH=false -v "$root/rclone:/config/rclone" "${opts[@]}" rclone/rclone:1.68.2 "$@"
}

if (($#)); then
  opts=(-it -v "$root/restore:/restore")
  rclone "$@"
  exit
fi

ping_url=$(env_get BACKUP_PING_URL)
notify() { [[ -z $ping_url ]] || curl -fsS -m 10 --retry 3 -o /dev/null "$ping_url$1" || true; }

work=$(mktemp -d)
trap 'rc=$?; rm -rf "$work"; if ((rc)); then log "LỖI ($rc)"; notify /fail; else notify ""; fi' EXIT

db_host='' db_name='' db_user='' db_pass=''
IFS=';' read -ra parts <<< "$(env_get DB_CONNECTION)"
for p in "${parts[@]}"; do
  key=${p%%=*}
  key=${key// /}
  val=${p#*=}
  case ${key,,} in
    host | server) db_host=$val ;;
    database | db) db_name=$val ;;
    username | userid | user) db_user=$val ;;
    password | pwd) db_pass=$val ;;
  esac
done
pg_container=$(env_get PG_CONTAINER)
pg_container=${pg_container:-$db_host}

log "pg_dump $db_name (container $pg_container)"
docker exec -e PGPASSWORD="$db_pass" "$pg_container" \
  pg_dump -U "$db_user" -d "$db_name" --format=custom > "$work/$db_name-$(date +%F).dump"

opts=(-v "$work:/db:ro" -v bosch-homevn_appdata:/appdata:ro -v "$env_file:/env/.env:ro")
log "Tải lên $dest"
rclone copy /db "$dest/db"
rclone copy /appdata "$dest/appdata"
rclone copy /env "$dest/env"
rclone delete --min-age 30d "$dest/db"

log "Xong"
