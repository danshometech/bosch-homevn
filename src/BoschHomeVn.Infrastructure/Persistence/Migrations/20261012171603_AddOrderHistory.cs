using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BoschHomeVn.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Actor = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FromStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderEvents_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderEvents_OrderId_CreatedAt",
                table: "OrderEvents",
                columns: new[] { "OrderId", "CreatedAt" });

            migrationBuilder.Sql("""
INSERT INTO "OrderEvents" ("OrderId", "CreatedAt", "ToStatus")
SELECT "Id", "CreatedAt", 'New' FROM "Orders";

INSERT INTO "OrderEvents" ("OrderId", "CreatedAt", "Actor", "Note")
SELECT "Id", "UpdatedAt", 'admin', left("StaffNote", 1000) FROM "Orders" WHERE "StaffNote" IS NOT NULL;
""");

            migrationBuilder.DropColumn(
                name: "StaffNote",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderEvents");

            migrationBuilder.AddColumn<string>(
                name: "StaffNote",
                table: "Orders",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
