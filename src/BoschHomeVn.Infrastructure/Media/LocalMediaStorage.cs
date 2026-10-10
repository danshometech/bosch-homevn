using BoschHomeVn.Application.Abstractions.Media;
using BoschHomeVn.Domain.Common;

namespace BoschHomeVn.Infrastructure.Media;

internal sealed class LocalMediaStorage(string rootPath, string requestPath) : IMediaStorage
{
    private static readonly string[] Folders = ["products", "moments", "articles", "news"];

    public async Task<string> SaveImageAsync(Stream content, string folder, CancellationToken cancellationToken)
    {
        if (!Folders.Contains(folder))
        {
            throw new DomainException($"Thư mục ảnh không hợp lệ: {folder}.");
        }

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.GetBuffer().AsSpan(0, (int)buffer.Length);
        var extension = Sniff(bytes) ?? throw new DomainException("Chỉ nhận ảnh JPG, PNG hoặc WebP.");

        var directory = Path.Combine(rootPath, folder);
        Directory.CreateDirectory(directory);
        var name = $"{Guid.NewGuid():N}{extension}";
        await File.WriteAllBytesAsync(Path.Combine(directory, name), buffer.ToArray(), cancellationToken);
        return $"{requestPath}/{folder}/{name}";
    }

    public async Task<string> SaveVideoAsync(Stream content, CancellationToken cancellationToken)
    {
        var head = new byte[12];
        var read = await content.ReadAtLeastAsync(head, head.Length, throwOnEndOfStream: false, cancellationToken);
        var extension = SniffVideo(head.AsSpan(0, read)) ?? throw new DomainException("Chỉ nhận video MP4 hoặc WebM.");

        var directory = Path.Combine(rootPath, "videos");
        Directory.CreateDirectory(directory);
        var name = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(directory, name);
        try
        {
            await using var file = File.Create(path);
            await file.WriteAsync(head.AsMemory(0, read), cancellationToken);
            await content.CopyToAsync(file, cancellationToken);
        }
        catch
        {
            File.Delete(path);
            throw;
        }
        return $"{requestPath}/videos/{name}";
    }

    private static string? SniffVideo(ReadOnlySpan<byte> b) =>
        b.Length >= 12 && b[4..8].SequenceEqual("ftyp"u8) && !b[8..12].SequenceEqual("qt  "u8) ? ".mp4"
        : b.Length >= 4 && b[..4].SequenceEqual(new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }) ? ".webm"
        : null;

    private static string? Sniff(ReadOnlySpan<byte> b) =>
        b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF ? ".jpg"
        : b.Length >= 8 && b[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }) ? ".png"
        : b.Length >= 12 && b[..4].SequenceEqual("RIFF"u8) && b[8..12].SequenceEqual("WEBP"u8) ? ".webp"
        : null;
}
