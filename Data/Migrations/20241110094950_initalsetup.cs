using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CouponCodes.Data.Migrations
{
    /// <inheritdoc />
    public partial class initalsetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Coupon",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<float>(
                name: "DiscountPercentage",
                table: "Coupon",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "Coupon",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStackable",
                table: "Coupon",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TimesUsed",
                table: "Coupon",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsageLimit",
                table: "Coupon",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "IsStackable",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "TimesUsed",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "UsageLimit",
                table: "Coupon");
        }
    }
}
