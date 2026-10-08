using BoschHomeVn.WebStore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<StoreApiClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? throw new InvalidOperationException("Thiếu cấu hình Api:BaseUrl."));
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { UseCookies = false });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

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
app.MapFallbackToFile("index.html", noCacheIndex);

app.Run();
