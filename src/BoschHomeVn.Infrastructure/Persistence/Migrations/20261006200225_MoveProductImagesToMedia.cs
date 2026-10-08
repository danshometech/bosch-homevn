using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoschHomeVn.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveProductImagesToMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ảnh sản phẩm chuyển từ public/images của site bán hàng (/images/sp-…) sang media của Api (/media/products/sp-…);
            // file gốc ở src/BoschHomeVn.Api/SeedMedia/products, Api chép sang thư mục media lúc khởi động
            migrationBuilder.Sql(
                "UPDATE Products SET ImageUrl = '/media/products/' + SUBSTRING(ImageUrl, LEN('/images/') + 1, 500) " +
                "WHERE ImageUrl LIKE '/images/sp-%'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE Products SET ImageUrl = '/images/' + SUBSTRING(ImageUrl, LEN('/media/products/') + 1, 500) " +
                "WHERE ImageUrl LIKE '/media/products/sp-%'");
        }
    }
}
