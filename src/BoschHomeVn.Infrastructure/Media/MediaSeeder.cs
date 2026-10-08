namespace BoschHomeVn.Infrastructure.Media;

// Ảnh gốc của dữ liệu seed (ảnh sản phẩm từ bảng giá) nằm trong repo ở SeedMedia/ của Api; thư mục media thật
// (Media:RootPath) không commit. Lúc Api khởi động chép những file media còn thiếu sang — không ghi đè file đã có,
// nên ảnh admin thay thế vẫn giữ nguyên.
public static class MediaSeeder
{
    // Trả về số file đã chép
    public static int CopyMissing(string sourceRoot, string mediaRoot)
    {
        if (!Directory.Exists(sourceRoot))
        {
            return 0;
        }

        var copied = 0;
        foreach (var source in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(mediaRoot, Path.GetRelativePath(sourceRoot, source));
            if (File.Exists(target))
            {
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(source, target);
            copied++;
        }
        return copied;
    }
}
