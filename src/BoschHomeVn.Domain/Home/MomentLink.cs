namespace BoschHomeVn.Domain.Home;

// Link dưới lời dẫn của khoảnh khắc, Url là đường dẫn trên site ("/san-pham/gia-dung?loai=may-giat")
public sealed record MomentLink(string Label, string Url);
