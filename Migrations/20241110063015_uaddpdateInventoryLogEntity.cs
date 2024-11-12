using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KhanhSkin_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class uaddpdateInventoryLogEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeInventory",
                table: "InventoryLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ItemPrice",
                table: "InventoryLogs",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductImage",
                table: "InventoryLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "InventoryLogs",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantImage",
                table: "InventoryLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeInventory",
                table: "InventoryLogs");

            migrationBuilder.DropColumn(
                name: "ItemPrice",
                table: "InventoryLogs");

            migrationBuilder.DropColumn(
                name: "ProductImage",
                table: "InventoryLogs");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "InventoryLogs");

            migrationBuilder.DropColumn(
                name: "VariantImage",
                table: "InventoryLogs");
        }
    }
}
