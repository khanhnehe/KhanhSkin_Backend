using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KhanhSkin_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class UpfateVoucherAndCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Vouchers_VoucherId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_VoucherId",
                table: "Carts");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_VoucherId",
                table: "Carts",
                column: "VoucherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Vouchers_VoucherId",
                table: "Carts",
                column: "VoucherId",
                principalTable: "Vouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
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

            migrationBuilder.CreateIndex(
                name: "IX_Carts_VoucherId",
                table: "Carts",
                column: "VoucherId",
                unique: true,
                filter: "[VoucherId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Vouchers_VoucherId",
                table: "Carts",
                column: "VoucherId",
                principalTable: "Vouchers",
                principalColumn: "Id");
        }
    }
}
