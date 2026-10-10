using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoschHomeVn.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductArticles",
                columns: table => new
                {
                    ProductId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Html = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductArticles", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_ProductArticles_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductArticles");
        }
    }
}
