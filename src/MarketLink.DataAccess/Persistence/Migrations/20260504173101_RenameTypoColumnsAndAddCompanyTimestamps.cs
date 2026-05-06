using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketLink.DataAccess.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameTypoColumnsAndAddCompanyTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SertificateUrl",
                table: "Shops",
                newName: "CertificateUrl");

            migrationBuilder.RenameColumn(
                name: "AvarageRaiting",
                table: "Products",
                newName: "AverageRating");

            migrationBuilder.RenameColumn(
                name: "SertificateUrl",
                table: "Companies",
                newName: "CertificateUrl");

            migrationBuilder.RenameColumn(
                name: "AvarageRaiting",
                table: "Companies",
                newName: "AverageRating");

            migrationBuilder.AlterColumn<string>(
                name: "ProductionType",
                table: "Companies",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "CertificateUrl",
                table: "Shops",
                newName: "SertificateUrl");

            migrationBuilder.RenameColumn(
                name: "AverageRating",
                table: "Products",
                newName: "AvarageRaiting");

            migrationBuilder.RenameColumn(
                name: "CertificateUrl",
                table: "Companies",
                newName: "SertificateUrl");

            migrationBuilder.RenameColumn(
                name: "AverageRating",
                table: "Companies",
                newName: "AvarageRaiting");

            migrationBuilder.AlterColumn<int>(
                name: "ProductionType",
                table: "Companies",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
