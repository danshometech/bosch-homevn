namespace BoschHomeVn.Application.Abstractions.Media;

// Nơi lưu ảnh admin tải lên (Infrastructure: thư mục trên đĩa, phục vụ qua /media/...)
public interface IMediaStorage
{
    // Chỉ nhận JPG / PNG / WebP (kiểm tra nội dung, không tin tên file); trả về đường dẫn công khai, vd. /media/products/ab12.jpg
    Task<string> SaveImageAsync(Stream content, string folder, CancellationToken cancellationToken);

    // Chỉ nhận MP4 / WebM (kiểm tra nội dung); lưu ở videos/, trả về đường dẫn công khai, vd. /media/videos/ab12.mp4
    Task<string> SaveVideoAsync(Stream content, CancellationToken cancellationToken);
}
