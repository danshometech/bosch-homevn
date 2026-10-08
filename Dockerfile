# syntax=docker/dockerfile:1
# Build: docker compose build   (hoặc docker build --target api|webstore|admin .)

FROM node:22-alpine AS web
WORKDIR /repo/web
COPY web/package.json web/package-lock.json ./
COPY web/webstore/package.json webstore/
COPY web/admin/package.json admin/
RUN npm ci
COPY web/ ./
# vite ghi thẳng vào /repo/src/BoschHomeVn.WebStore/wwwroot và /repo/src/BoschHomeVn.Admin/wwwroot
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY Directory.Build.props Directory.Packages.props BoschHomeVn.sln ./
COPY src/BoschHomeVn.Api/BoschHomeVn.Api.csproj src/BoschHomeVn.Api/
COPY src/BoschHomeVn.WebStore/BoschHomeVn.WebStore.csproj src/BoschHomeVn.WebStore/
COPY src/BoschHomeVn.Admin/BoschHomeVn.Admin.csproj src/BoschHomeVn.Admin/
COPY src/BoschHomeVn.Application/BoschHomeVn.Application.csproj src/BoschHomeVn.Application/
COPY src/BoschHomeVn.Domain/BoschHomeVn.Domain.csproj src/BoschHomeVn.Domain/
COPY src/BoschHomeVn.Infrastructure/BoschHomeVn.Infrastructure.csproj src/BoschHomeVn.Infrastructure/
COPY src/BoschHomeVn.Contracts/BoschHomeVn.Contracts.csproj src/BoschHomeVn.Contracts/
RUN dotnet restore BoschHomeVn.sln
COPY src/ src/
COPY --from=web /repo/src/BoschHomeVn.WebStore/wwwroot src/BoschHomeVn.WebStore/wwwroot
COPY --from=web /repo/src/BoschHomeVn.Admin/wwwroot src/BoschHomeVn.Admin/wwwroot
RUN dotnet publish src/BoschHomeVn.Api -c Release -o /out/api --no-restore \
 && dotnet publish src/BoschHomeVn.WebStore -c Release -o /out/webstore --no-restore \
 && dotnet publish src/BoschHomeVn.Admin -c Release -o /out/admin --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

FROM runtime AS api
COPY --from=build /out/api .
# App_Data (ảnh / video tải lên + khóa đăng nhập) gắn volume; tạo sẵn với quyền của user không phải root
RUN mkdir -p /app/App_Data && chown -R $APP_UID /app/App_Data
USER $APP_UID
ENTRYPOINT ["dotnet", "BoschHomeVn.Api.dll"]

FROM runtime AS webstore
COPY --from=build /out/webstore .
USER $APP_UID
ENTRYPOINT ["dotnet", "BoschHomeVn.WebStore.dll"]

FROM runtime AS admin
COPY --from=build /out/admin .
USER $APP_UID
ENTRYPOINT ["dotnet", "BoschHomeVn.Admin.dll"]
