using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KhanhSkin_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToCartNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountValue",
                table: "Carts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalPrice",
                table: "Carts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "VoucherId",
                table: "Carts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_VoucherId",
                table: "Carts",
                column: "VoucherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Vouchers_VoucherId",
                table: "Carts",
                column: "VoucherId",
                principalTable: "Vouchers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Vouchers_VoucherId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_VoucherId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "DiscountValue",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "FinalPrice",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "VoucherId",
                table: "Carts");
        }
    }
}
