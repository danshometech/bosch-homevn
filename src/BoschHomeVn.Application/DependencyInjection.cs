using BoschHomeVn.Application.Catalog.Admin;
using BoschHomeVn.Application.Catalog.GetMenu;
using BoschHomeVn.Application.Catalog.GetProductById;
using BoschHomeVn.Application.Catalog.GetProducts;
using BoschHomeVn.Application.Dashboard;
using BoschHomeVn.Application.Home.Admin;
using BoschHomeVn.Application.Home.GetHomePage;
using Microsoft.Extensions.DependencyInjection;

namespace BoschHomeVn.Application;

public static class DependencyInjection
{
    // Handler của từng use case đăng ký ở đây — không dùng MediatR
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<GetMenuHandler>();
        services.AddScoped<GetProductsHandler>();
        services.AddScoped<GetProductByIdHandler>();
        services.AddScoped<GetHomePageHandler>();

        // Trang quản trị
        services.AddScoped<ProductAdminHandler>();
        services.AddScoped<CategoryAdminHandler>();
        services.AddScoped<MomentAdminHandler>();
        services.AddScoped<GetDashboardHandler>();

        return services;
    }
}
