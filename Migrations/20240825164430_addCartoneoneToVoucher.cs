using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KhanhSkin_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class addCartoneoneToVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Carts_VoucherId",
                table: "Carts");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_VoucherId",
                table: "Carts",
                column: "VoucherId",
                unique: true,
                filter: "[VoucherId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Carts_VoucherId",
                table: "Carts");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_VoucherId",
                table: "Carts",
                column: "VoucherId");
        }
    }
}
