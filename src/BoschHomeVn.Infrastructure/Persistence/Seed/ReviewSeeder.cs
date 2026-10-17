using System.Globalization;
using BoschHomeVn.Domain.Catalog;
using BoschHomeVn.Domain.Reviews;
using BoschHomeVn.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Infrastructure.Persistence.Seed;

// Đánh giá mẫu cho demo / thử giao diện (3–7 đánh giá đã duyệt cho mỗi sản phẩm, không có ảnh / số điện thoại),
// CreatedBy = "seed". Bật Seed:SampleReviews: tạo một lần (đánh dấu bằng khóa seed.sample-reviews trong SiteSettings).
// Tắt: xóa hết đánh giá mẫu đã tạo — site thật không bao giờ giữ lại dữ liệu mẫu
internal static class ReviewSeeder
{
    public const string Actor = "seed";
    private const string MarkerKey = "seed.sample-reviews";

    public static void Seed(DbContext context, bool enabled)
    {
        var db = (AppDbContext)context;
        var marker = db.SiteSettings.FirstOrDefault(s => s.Key == MarkerKey);
        if (enabled == marker is not null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (enabled)
        {
            foreach (var t in Targets(db).ToList())
            {
                db.ProductReviews.AddRange(Build(t.ProductId, GroupOf(t.TypeId, t.Name), now));
            }
            db.SiteSettings.Add(new SiteSetting(MarkerKey, now.ToString("O", CultureInfo.InvariantCulture)));
        }
        else
        {
            db.ProductReviews.Where(r => r.CreatedBy == Actor).ExecuteDelete();
            db.SiteSettings.Remove(marker!);
        }
        db.SaveChanges();

        var stats = Stats(db).ToDictionary(x => x.ProductId);
        Refresh(db.Products.ToList(), stats);
        db.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, bool enabled, CancellationToken cancellationToken)
    {
        var db = (AppDbContext)context;
        var marker = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == MarkerKey, cancellationToken);
        if (enabled == marker is not null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (enabled)
        {
            foreach (var t in await Targets(db).ToListAsync(cancellationToken))
            {
                db.ProductReviews.AddRange(Build(t.ProductId, GroupOf(t.TypeId, t.Name), now));
            }
            db.SiteSettings.Add(new SiteSetting(MarkerKey, now.ToString("O", CultureInfo.InvariantCulture)));
        }
        else
        {
            await db.ProductReviews.Where(r => r.CreatedBy == Actor).ExecuteDeleteAsync(cancellationToken);
            db.SiteSettings.Remove(marker!);
        }
        await db.SaveChangesAsync(cancellationToken);

        var stats = await Stats(db).ToDictionaryAsync(x => x.ProductId, cancellationToken);
        Refresh(await db.Products.ToListAsync(cancellationToken), stats);
        await db.SaveChangesAsync(cancellationToken);
    }

    private sealed record Target(string ProductId, string TypeId, string Name);

    private sealed record Stat(string ProductId, int Count, int Sum);

    private static IQueryable<Target> Targets(AppDbContext db) =>
        db.Products.OrderBy(p => p.Id).Select(p => new Target(p.Id, p.ProductTypeId, p.Name));

    private static IQueryable<Stat> Stats(AppDbContext db) =>
        db.ProductReviews
            .Where(r => r.Status == ReviewStatus.Approved)
            .GroupBy(r => r.ProductId)
            .Select(g => new Stat(g.Key, g.Count(), g.Sum(r => r.Rating)));

    // Điểm trung bình + số đánh giá của mọi sản phẩm theo các đánh giá đang hiện (cả đánh giá thật nếu có)
    private static void Refresh(IEnumerable<Product> products, Dictionary<string, Stat> stats)
    {
        foreach (var p in products)
        {
            p.SetRating(stats.TryGetValue(p.Id, out var s) ? Math.Round((decimal)s.Sum / s.Count, 1) : null, s?.Count ?? 0);
        }
    }

    private static List<ProductReview> Build(string productId, string group, DateTime now)
    {
        var rng = new Random(StableHash(productId));
        var praise = new Pool(Praise[group], rng);
        var minor = new Pool(Minor[group], rng);
        var service = new Pool(Service, rng);
        var bad = new Pool(Bad, rng);
        var names = new Pool(Names, rng);

        var count = rng.Next(3, 8);
        var list = new List<ProductReview>(count);
        for (var i = 0; i < count; i++)
        {
            var stars = StarsOf(rng.Next(100));
            var text = stars switch
            {
                5 => rng.Next(10) < 7 ? $"{praise.Next()} {service.Next()}" : $"{praise.Next()} {praise.Next()}",
                4 => $"{praise.Next()} {minor.Next()}",
                3 => $"{minor.Next()} {Mixed[rng.Next(Mixed.Length)]}",
                _ => $"{bad.Next()} Sản phẩm thì dùng ổn.",
            };
            var createdAt = now.AddDays(-rng.Next(2, 180)).AddMinutes(-rng.Next(24 * 60));
            var review = new ProductReview(
                productId, stars, text, names.Next(), null, [], rng.Next(100) < 75, ReviewStatus.Approved, Actor, createdAt);

            var reply = stars switch
            {
                <= 2 => BadReplies[rng.Next(BadReplies.Length)],
                <= 4 => rng.Next(2) == 0 ? MinorReplies[rng.Next(MinorReplies.Length)] : null,
                _ => rng.Next(4) == 0 ? ThanksReplies[rng.Next(ThanksReplies.Length)] : null,
            };
            var repliedAt = createdAt.AddHours(rng.Next(2, 30));
            review.SetReply(reply, repliedAt < now ? repliedAt : now);
            review.AddHelpful(rng.Next(4) * rng.Next(4));
            list.Add(review);
        }
        return list;
    }

    private static int StarsOf(int roll) => roll switch
    {
        < 62 => 5,
        < 88 => 4,
        < 96 => 3,
        < 98 => 2,
        _ => 1,
    };

    // Lấy lần lượt từ danh sách đã xáo trộn, hết thì quay lại đầu
    private sealed class Pool(string[] items, Random rng)
    {
        private readonly string[] _items = [.. items.OrderBy(_ => rng.Next())];
        private int _next;

        public string Next() => _items[_next++ % _items.Length];
    }

    // FNV-1a: cùng mã sản phẩm luôn ra cùng bộ đánh giá (string.GetHashCode đổi theo mỗi lần chạy)
    private static int StableHash(string value)
    {
        var hash = 2166136261u;
        foreach (var c in value)
        {
            hash = (hash ^ c) * 16777619u;
        }
        return (int)(hash & 0x7FFFFFFF);
    }

    // Lò vi sóng nằm chung loại "Lò nướng & Lò vi sóng" vẫn lấy câu đánh giá của lò vi sóng
    private static string GroupOf(string typeId, string name) =>
        name.Normalize().Contains("vi sóng", StringComparison.OrdinalIgnoreCase) ? "lo-vi-song" : typeId switch
    {
        "bep-tu" or "bep-gas" or "lo-nuong" or "lo-vi-song" or "xoong-noi" or "may-hut-mui" or "may-rua-bat"
            or "may-pha-ca-phe" or "may-hut-bui" or "khoa-cua-van-tay" => typeId,
        "tu-lanh" or "tu-bao-quan-ruou" => "tu-lanh",
        "chau-rua-chen-bat" or "voi-rua-bat" => "so-che",
        "may-giat" or "may-say-quan-ao" => "giat-say",
        _ => "nho",
    };

    private static readonly Dictionary<string, string[]> Praise = new()
    {
        ["bep-tu"] =
        [
            "Bếp lên nhiệt rất nhanh, đun nồi nước to chỉ vài phút là sôi. Mặt kính lau một lượt là sạch.",
            "Chức năng hẹn giờ dùng rất tiện, nấu cháo cho con không phải đứng canh.",
            "Bếp chạy êm, quạt tản nhiệt nhỏ tiếng, các nút cảm ứng nhạy và dễ dùng.",
            "Vùng nấu rộng nên đặt chảo to hay nồi nhỏ đều được, nấu cả buổi không thấy nóng bếp.",
            "Có khóa trẻ em và tự ngắt khi nhấc nồi ra, nhà có con nhỏ thấy yên tâm hơn hẳn bếp cũ.",
        ],
        ["bep-gas"] =
        [
            "Lửa xanh và đều, đánh lửa ăn ngay, xào nấu món Việt rất hợp.",
            "Mặt kính đẹp, kiềng gang chắc chắn, đặt nồi to vẫn vững.",
            "Có van an toàn tự ngắt gas khi lửa tắt, dùng rất yên tâm.",
            "Núm vặn chỉnh lửa nhỏ rất mịn, hầm hay kho đều được.",
        ],
        ["lo-nuong"] =
        [
            "Lò nướng chín đều, bánh lên màu đẹp, không bị cháy một bên như lò cũ.",
            "Nhiều chế độ nướng có sẵn, người mới tập làm bánh như mình cũng dùng được.",
            "Khoang lò rộng, nướng nguyên con gà vẫn thoải mái. Cửa lò cách nhiệt tốt, bên ngoài không quá nóng.",
            "Lắp âm tủ nhìn rất gọn bếp, đèn trong lò sáng nên dễ theo dõi món nướng.",
        ],
        ["lo-vi-song"] =
        [
            "Hâm nóng và rã đông nhanh, đều, không bị chỗ nóng chỗ lạnh.",
            "Thiết kế đẹp, lắp âm tủ nhìn rất gọn bếp.",
            "Các chế độ cài sẵn dễ dùng, bố mẹ lớn tuổi trong nhà cũng tự hâm đồ ăn được.",
            "Khoang lò dễ lau chùi, dùng mấy tháng vẫn sạch như mới.",
        ],
        ["xoong-noi"] =
        [
            "Nồi dày dặn, đáy truyền nhiệt đều, nấu trên bếp từ rất nhanh.",
            "Inox sáng đẹp, nắp kính nhìn được thức ăn bên trong, rửa cũng dễ.",
            "Bộ nồi đủ kích cỡ, tay cầm chắc và không bị nóng.",
            "Dùng hằng ngày mấy tháng rồi đáy nồi vẫn phẳng, không cong vênh.",
        ],
        ["may-hut-mui"] =
        [
            "Hút khói rất mạnh, chiên cá mà phòng khách không còn ám mùi.",
            "Chạy ở mức thấp khá êm, đèn LED sáng rõ cả mặt bếp.",
            "Lưới lọc tháo ra rửa dễ, cho vào máy rửa bát cũng được.",
            "Thiết kế đẹp, lắp xong nhìn góc bếp sang hẳn lên.",
        ],
        ["may-rua-bat"] =
        [
            "Rửa rất sạch, nồi chảo bám dầu mỡ cũng ra sạch bóng. Bát đĩa sấy khô ráo.",
            "Máy chạy êm, buổi tối bật vẫn nói chuyện bình thường.",
            "Tiết kiệm nước hơn rửa tay nhiều, nhà đông người mỗi ngày chạy một lần là đủ.",
            "Giàn trên chỉnh độ cao được, xếp ly cao hay nồi to đều vừa.",
        ],
        ["tu-lanh"] =
        [
            "Tủ làm lạnh nhanh và đều, rau để cả tuần vẫn tươi.",
            "Chạy rất êm, đặt trong bếp liền phòng khách cũng không nghe tiếng.",
            "Ngăn kéo rộng, kệ kính chắc chắn, sắp xếp đồ ăn rất gọn.",
            "Tủ tiết kiệm điện, hóa đơn tháng đầu không tăng mấy so với tủ cũ nhỏ hơn.",
        ],
        ["so-che"] =
        [
            "Inox dày dặn, hoàn thiện đẹp, lắp vào bếp nhìn rất sang.",
            "Vòi xoay linh hoạt, nước ra mạnh và đều, chuyển chế độ phun rất tiện.",
            "Chậu sâu và rộng, rửa nồi to thoải mái, xả nước không bị kêu.",
            "Dùng mấy tháng vẫn sáng bóng, không bị ố hay hoen gỉ.",
        ],
        ["giat-say"] =
        [
            "Giặt sạch, quần áo ra ít nhăn, chế độ giặt nhanh rất hợp ngày bận.",
            "Máy vắt êm, gần như không rung dù đặt ở ban công.",
            "Có chế độ cho đồ em bé và chăn mền, nhà có con nhỏ dùng rất tiện.",
            "Quần áo ra thơm và mềm, mùa nồm không lo đồ bị ẩm mùi.",
        ],
        ["may-pha-ca-phe"] =
        [
            "Cà phê pha ra thơm, lớp crema đẹp, sáng nào cũng có ly ngon trong vài phút.",
            "Máy nhỏ gọn, thao tác đơn giản, vệ sinh cũng nhanh.",
            "Pha được nhiều loại đồ uống, cả nhà ai thích gì tự pha nấy.",
        ],
        ["may-hut-bui"] =
        [
            "Lực hút mạnh, lông chó mèo trên thảm cũng hút sạch.",
            "Máy nhẹ, cầm tay hút gầm giường, sofa rất tiện.",
            "Pin dùng đủ cho cả căn hộ, sạc cũng nhanh.",
            "Đầu hút xoay linh hoạt, len vào góc tủ dễ dàng.",
        ],
        ["khoa-cua-van-tay"] =
        [
            "Nhận vân tay rất nhanh, tay hơi ướt cũng mở được.",
            "Có nhiều cách mở: vân tay, mã số, thẻ từ, chìa cơ — người lớn tuổi trong nhà dùng cũng dễ.",
            "Khóa chắc chắn, lắp lên cửa nhìn hiện đại hẳn.",
            "Báo pin yếu từ sớm, thay pin đơn giản, dùng mấy tháng vẫn chưa phải thay.",
        ],
        ["nho"] =
        [
            "Máy chạy khỏe, thao tác đơn giản, cả nhà ai cũng dùng được.",
            "Thiết kế chắc chắn, chất liệu tốt, cầm lên là thấy hàng chất lượng.",
            "Các bộ phận tháo lắp dễ, rửa nhanh rồi cất gọn trong tủ.",
            "Dùng hằng ngày mấy tháng vẫn chạy êm và ổn định.",
        ],
    };

    private static readonly Dictionary<string, string[]> Minor = new()
    {
        ["bep-tu"] = ["Chỉ tiếc là phải dùng nồi đáy từ, mấy cái nồi cũ trong nhà không dùng được.", "Nút cảm ứng hơi nhạy, lau bếp lúc đang nấu dễ chạm nhầm."],
        ["bep-gas"] = ["Kiềng gang hơi nặng, mỗi lần tháo ra lau hơi mất công.", "Mặt kính dễ để lại vết nước, phải lau khô sau khi nấu."],
        ["lo-nuong"] = ["Lần đầu dùng có mùi máy mới, chạy không tải một lần là hết.", "Bảng điều khiển nhiều chức năng, phải đọc hướng dẫn một lúc mới quen."],
        ["lo-vi-song"] = ["Tiếng bíp báo xong hơi to.", "Bảng điều khiển nhiều chức năng, phải đọc hướng dẫn một lúc mới quen."],
        ["xoong-noi"] = ["Nồi hơi nặng tay so với nồi nhôm mình dùng trước đây.", "Đun lửa to dễ bị ố vàng đáy, phải chịu khó vệ sinh."],
        ["may-hut-mui"] = ["Mức hút cao nhất hơi ồn, bù lại hút rất sạch.", "Cần đi ống thoát khí ra ngoài, nhà chung cư nên tính trước lúc làm tủ bếp."],
        ["may-rua-bat"] = ["Chương trình Eco chạy hơi lâu, mình thường hẹn giờ cho máy chạy ban đêm.", "Phải mua muối và nước làm bóng riêng, lúc đầu chưa quen."],
        ["tu-lanh"] = ["Cửa tủ hơi nặng, lúc đầu phải quen.", "Kích thước khá lớn, nên đo kỹ chỗ đặt trước khi mua."],
        ["so-che"] = ["Lắp đặt cần thợ có kinh nghiệm để cắt mặt đá cho chuẩn.", "Bề mặt dễ thấy vết nước, phải lau khô thường xuyên."],
        ["giat-say"] = ["Chu trình tiêu chuẩn chạy hơi lâu, mình hay chọn chế độ nhanh.", "Nhiều chế độ quá, phải đọc sách hướng dẫn mới dùng hết."],
        ["may-pha-ca-phe"] = ["Lúc khởi động máy hơi ồn một chút.", "Phải chọn đúng loại cà phê phù hợp với máy, lúc đầu mình mua chưa đúng."],
        ["may-hut-bui"] = ["Hộp chứa bụi hơi nhỏ, nhà rộng phải đổ giữa chừng.", "Chế độ mạnh nhất hơi ồn và tốn pin."],
        ["khoa-cua-van-tay"] = ["Cài đặt ban đầu hơi nhiều bước, nhờ kỹ thuật viên hướng dẫn mới xong.", "Tiếng bíp khi mở cửa hơi to, muốn nhỏ hơn phải vào cài đặt."],
        ["nho"] = ["Chạy ở tốc độ cao hơi ồn một chút.", "Dây điện hơi ngắn, phải đặt gần ổ cắm."],
    };

    private static readonly string[] Service =
    [
        "Giao hàng đúng hẹn, nhân viên lắp đặt cẩn thận và hướng dẫn kỹ.",
        "Hàng chính hãng, có bảo hành điện tử đầy đủ.",
        "Tư vấn nhiệt tình, chọn được đúng mẫu hợp với nhà mình.",
        "Đóng gói kỹ, sản phẩm nguyên vẹn, giao nhanh hơn dự kiến.",
        "Giá tốt so với nhiều nơi mình tham khảo.",
        "Mua lần thứ hai ở cửa hàng, vẫn hài lòng như lần trước.",
    ];

    private static readonly string[] Mixed =
    [
        "Nhìn chung dùng tạm ổn so với giá.",
        "Sản phẩm ổn nhưng chưa thật sự vượt mong đợi.",
        "Sẽ dùng thêm một thời gian rồi đánh giá lại.",
    ];

    private static readonly string[] Bad =
    [
        "Giao hàng trễ hai ngày so với lịch hẹn mà không được báo trước.",
        "Hẹn lắp đặt mấy lần mới có thợ đến, khá mất thời gian.",
        "Thùng hàng bị móp khi nhận, may là sản phẩm bên trong không sao.",
        "Gọi tổng đài hỏi cách sử dụng phải chờ khá lâu.",
    ];

    private static readonly string[] ThanksReplies =
    [
        "Cảm ơn anh/chị đã tin chọn Bosch và dành thời gian chia sẻ. Chúc gia đình mình dùng sản phẩm thật hài lòng!",
        "Cảm ơn anh/chị đã đánh giá. Cần hỗ trợ gì trong quá trình sử dụng, anh/chị cứ gọi hotline nhé.",
    ];

    private static readonly string[] MinorReplies =
    [
        "Cảm ơn anh/chị đã góp ý chi tiết. Nếu cần hướng dẫn thêm, anh/chị gọi hotline để kỹ thuật viên hỗ trợ nhé.",
        "Cửa hàng đã ghi nhận góp ý của anh/chị. Cảm ơn anh/chị đã tin chọn Bosch!",
    ];

    private static readonly string[] BadReplies =
    [
        "Cửa hàng thành thật xin lỗi anh/chị về trải nghiệm chưa tốt. Bộ phận chăm sóc khách hàng đã liên hệ để ghi nhận và khắc phục.",
        "Cửa hàng xin lỗi anh/chị vì sự bất tiện này và đã chấn chỉnh lại khâu giao nhận. Cảm ơn anh/chị đã góp ý.",
    ];

    private static readonly string[] Names =
    [
        "Nguyễn Minh Anh", "Trần Quốc Huy", "Lê Thu Hà", "Phạm Đức Thắng", "Hoàng Ngọc Lan", "Vũ Thanh Tùng", "Đặng Hải Yến",
        "Bùi Văn Long", "Đỗ Khánh Linh", "Hồ Gia Bảo", "Ngô Thị Mai", "Dương Tuấn Kiệt", "Lý Bảo Ngọc", "Phan Minh Khoa",
        "Võ Thị Hồng", "Trịnh Quang Vinh", "Đinh Phương Thảo", "Mai Anh Tuấn", "Tạ Thu Trang", "Lương Hoàng Nam",
        "Chu Thị Hạnh", "Cao Văn Sơn", "Hà Mỹ Duyên", "Kiều Đức Anh", "Lâm Ngọc Hân", "Quách Thành Đạt", "Tô Thị Nga",
        "Văn Hữu Phước", "Châu Bích Thủy", "Nguyễn Hoàng Phúc", "Trần Thị Bích", "Lê Văn Hùng", "Phạm Thu Hương",
        "Hoàng Minh Tâm", "Vũ Ngọc Ánh", "Đặng Quốc Bảo", "Bùi Thị Thanh", "Đỗ Mạnh Cường", "Nguyễn Thảo Vy", "Trần Đình Khôi",
    ];
}
