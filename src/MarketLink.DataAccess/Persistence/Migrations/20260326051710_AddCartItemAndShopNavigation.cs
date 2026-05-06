using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MarketLink.DataAccess.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCartItemAndShopNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ratings_ProductId_ShopId_OrderId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_ShopId",
                table: "Ratings");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Shops",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AvarageRaiting",
                table: "Products",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShopId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ProductId",
                table: "Ratings",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ShopId_ProductId_OrderId",
                table: "Ratings",
                columns: new[] { "ShopId", "ProductId", "OrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ShopId_ProductId",
                table: "CartItems",
                columns: new[] { "ShopId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_ProductId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_ShopId_ProductId_OrderId",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "AvarageRaiting",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ProductId_ShopId_OrderId",
                table: "Ratings",
                columns: new[] { "ProductId", "ShopId", "OrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ShopId",
                table: "Ratings",
                column: "ShopId");
        }
    }
}
