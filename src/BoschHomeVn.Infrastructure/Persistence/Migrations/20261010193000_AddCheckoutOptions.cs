using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoschHomeVn.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsInstallment = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShippingMethods",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Fee = table.Column<decimal>(type: "numeric(18,0)", precision: 18, scale: 0, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingMethods", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ShippingMethods",
                columns: new[] { "Id", "Name", "Description", "Fee", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { "giao-tieu-chuan", "Giao tiêu chuẩn + lắp đặt", "2–3 ngày · Kỹ thuật viên lắp đặt tận nơi", 0m, true, 0 },
                    { "giao-nhanh-24h", "Giao nhanh trong 24h", "Áp dụng nội thành HN, HCM, ĐN", 50000m, true, 1 },
                    { "nhan-tai-showroom", "Nhận tại showroom", "12 showroom toàn quốc", 0m, true, 2 },
                });

            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "Id", "Name", "Description", "IsInstallment", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { "cod", "Thanh toán khi nhận hàng (COD)", "Kiểm tra hàng trước khi thanh toán", false, true, 0 },
                    { "chuyen-khoan", "Chuyển khoản / QR ngân hàng", "VietQR · Xác nhận tự động", false, true, 1 },
                    { "vi-dien-tu", "Ví điện tử", "MoMo, ZaloPay, VNPay", false, true, 2 },
                    { "the-quoc-te", "Thẻ quốc tế", "Visa, Mastercard, JCB", false, true, 3 },
                    { "tra-gop", "Trả góp 0% qua thẻ tín dụng", "25 ngân hàng · Không cần trả trước", true, true, 4 },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "ShippingMethods");
        }
    }
}
