using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Repository.MSSql.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerAvatarBlob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarImageUrl",
                table: "Sellers");

            migrationBuilder.AddColumn<Guid>(
                name: "AvatarId",
                table: "Sellers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sellers_AvatarId",
                table: "Sellers",
                column: "AvatarId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sellers_Blobs_AvatarId",
                table: "Sellers",
                column: "AvatarId",
                principalTable: "Blobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sellers_Blobs_AvatarId",
                table: "Sellers");

            migrationBuilder.DropIndex(
                name: "IX_Sellers_AvatarId",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "Sellers");

            migrationBuilder.AddColumn<string>(
                name: "AvatarImageUrl",
                table: "Sellers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
