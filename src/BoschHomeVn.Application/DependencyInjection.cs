using BoschHomeVn.Application.Catalog.Admin;
using BoschHomeVn.Application.Catalog.GetMenu;
using BoschHomeVn.Application.Catalog.GetProductArticle;
using BoschHomeVn.Application.Catalog.GetProductById;
using BoschHomeVn.Application.Catalog.GetProductListing;
using BoschHomeVn.Application.Catalog.GetProducts;
using BoschHomeVn.Application.Checkout;
using BoschHomeVn.Application.Checkout.Admin;
using BoschHomeVn.Application.Dashboard;
using BoschHomeVn.Application.Home.Admin;
using BoschHomeVn.Application.Home.GetHomePage;
using BoschHomeVn.Application.News;
using BoschHomeVn.Application.News.Admin;
using BoschHomeVn.Application.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace BoschHomeVn.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<GetMenuHandler>();
        services.AddScoped<GetProductsHandler>();
        services.AddScoped<GetProductListingHandler>();
        services.AddScoped<GetProductByIdHandler>();
        services.AddScoped<GetProductArticleHandler>();
        services.AddScoped<GetHomePageHandler>();
        services.AddScoped<NewsHandler>();
        services.AddScoped<SiteSettingsHandler>();
        services.AddScoped<CheckoutOptionsHandler>();

        services.AddScoped<ProductAdminHandler>();
        services.AddScoped<CategoryAdminHandler>();
        services.AddScoped<MomentAdminHandler>();
        services.AddScoped<GetDashboardHandler>();
        services.AddScoped<NewsAdminHandler>();
        services.AddScoped<CheckoutAdminHandler>();

        return services;
    }
}
