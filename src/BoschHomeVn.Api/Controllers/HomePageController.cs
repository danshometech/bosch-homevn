using BoschHomeVn.Api.Mappings;
using BoschHomeVn.Application.Home.GetHomePage;
using BoschHomeVn.Contracts.Home;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers;

[ApiController]
[Route("api/home")]
public sealed class HomePageController : ControllerBase
{
    // Trang chủ chỉ gọi endpoint này: các khoảnh khắc "A Day with Bosch" kèm sản phẩm, và tóm tắt flash sale
    [HttpGet]
    public async Task<HomePageResponse> Get([FromServices] GetHomePageHandler handler, CancellationToken cancellationToken)
    {
        var page = await handler.Handle(cancellationToken);
        var flash = page.FlashSale is { } f ? new FlashSaleResponse(f.Count, f.MaxDiscountPercent, f.TypeNames) : null;
        return new HomePageResponse([.. page.Moments.Select(ToResponse)], flash);
    }

    private static HomeMomentResponse ToResponse(HomeMomentItem item)
    {
        var m = item.Moment;
        var more = m.ExtrasMoreLabel is { } label && m.ExtrasMoreUrl is { } url ? new LinkResponse(label, url) : null;
        return new HomeMomentResponse(
            m.Id,
            m.Time,
            m.Name,
            m.Tone,
            m.Title,
            m.Lead,
            m.ImageUrl,
            m.ImagePosition,
            m.StillImageUrl,
            m.ImageAlt,
            item.FactValue is { } value ? new MomentFactResponse(value, m.FactNote) : null,
            [.. m.Links.Select(l => new LinkResponse(l.Label, l.Url))],
            [.. item.Products.Select(p => p.ToResponse())],
            new MomentExtrasResponse(m.ExtrasTitle, [.. item.Extras.Select(p => p.ToResponse())], more));
    }
}
