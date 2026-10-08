using BoschHomeVn.Admin.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Vue (trang quản trị) chỉ gọi host này (/api/admin/*, /media/*); controller trong Controllers/ gọi tiếp BoschHomeVn.Api
// qua AdminApiClient. Lỗi từ Api trả nguyên mã + ProblemDetails về Vue (ApiCall).
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ApiCookieRelayHandler>();
builder.Services.AddHttpClient<AdminApiClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? throw new InvalidOperationException("Thiếu cấu hình Api:BaseUrl."));
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    // Handler được dùng lại giữa các request nên không giữ cookie chung; cookie đăng nhập do ApiCookieRelayHandler chuyển
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { UseCookies = false })
    .AddHttpMessageHandler<ApiCookieRelayHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

// wwwroot là output build của web/admin (vite), có thể đổi sau khi dotnet build
// nên đọc thẳng từ đĩa bằng UseStaticFiles thay vì MapStaticAssets (manifest tạo lúc build)
// index.html luôn hỏi lại server: mỗi lần build Vue đổi tên file js/css, index.html cũ trong cache sẽ trỏ tới file không còn
var noCacheIndex = new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name == "index.html")
        {
            ctx.Context.Response.Headers.CacheControl = "no-cache";
        }
    },
};
app.UseStaticFiles(noCacheIndex);
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
// Đường dẫn không khớp file tĩnh hay controller → index.html để Vue Router (history mode) xử lý
app.MapFallbackToFile("index.html", noCacheIndex);

app.Run();
