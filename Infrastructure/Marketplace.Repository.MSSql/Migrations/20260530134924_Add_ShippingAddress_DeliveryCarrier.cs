using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Repository.MSSql.Migrations
{
    /// <inheritdoc />
    public partial class Add_ShippingAddress_DeliveryCarrier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingCity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCountry",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingLine1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingLine2",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingPhone",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingPostalCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingRecipientName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingState",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "Carrier",
                table: "Shipments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ShippingAddressId",
                table: "Shipments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "DeliveryCarrier",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShippingAddressId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ShippingAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Line1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Line2 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    City = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    State = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingAddresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShippingAddressId",
                table: "Shipments",
                column: "ShippingAddressId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingAddressId",
                table: "Orders",
                column: "ShippingAddressId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ShippingAddresses_ShippingAddressId",
                table: "Orders",
                column: "ShippingAddressId",
                principalTable: "ShippingAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_ShippingAddresses_ShippingAddressId",
                table: "Shipments",
                column: "ShippingAddressId",
                principalTable: "ShippingAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ShippingAddresses_ShippingAddressId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_ShippingAddresses_ShippingAddressId",
                table: "Shipments");

            migrationBuilder.DropTable(
                name: "ShippingAddresses");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_ShippingAddressId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShippingAddressId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Carrier",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShippingAddressId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DeliveryCarrier",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddressId",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "ShippingCity",
                table: "Orders",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingCountry",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingLine1",
                table: "Orders",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingLine2",
                table: "Orders",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingPhone",
                table: "Orders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingPostalCode",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingRecipientName",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingState",
                table: "Orders",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }
    }
}
