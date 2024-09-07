using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KhanhSkin_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class updatedOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoucherDiscountSnapshot",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VoucherSnapshot",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "VoucherDiscountSnapshot",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoucherSnapshot",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
