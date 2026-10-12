using BoschHomeVn.Application.Home.Hero;
using BoschHomeVn.Contracts.Admin;
using BoschHomeVn.Contracts.Home;

namespace BoschHomeVn.Api.Mappings;

internal static class HomeMappings
{
    public static HomeHeroResponse ToResponse(this HomeHero h) =>
        new(h.Time, h.Kicker, h.Title, h.Lead, h.CtaLabel, h.CtaUrl, h.ImageUrl, h.ImagePosition, h.ImageAlt);

    public static HomeHero ToHero(this SaveHomeHeroRequest r) =>
        new(r.Time, r.Kicker, r.Title, r.Lead, r.CtaLabel, r.CtaUrl, r.ImageUrl ?? "", r.ImagePosition, r.ImageAlt);
}
