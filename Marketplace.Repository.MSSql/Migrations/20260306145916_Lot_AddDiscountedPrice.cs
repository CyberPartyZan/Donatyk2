using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Repository.MSSql.Migrations
{
    /// <inheritdoc />
    public partial class Lot_AddDiscountedPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Lots");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountedPrice_Amount",
                table: "Lots",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountedPrice_Currency",
                table: "Lots",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountedPrice_Amount",
                table: "Lots");

            migrationBuilder.DropColumn(
                name: "DiscountedPrice_Currency",
                table: "Lots");

            migrationBuilder.AddColumn<double>(
                name: "Discount",
                table: "Lots",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
