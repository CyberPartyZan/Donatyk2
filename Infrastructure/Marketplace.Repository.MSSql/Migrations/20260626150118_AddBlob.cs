using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Repository.MSSql.Migrations
{
    /// <inheritdoc />
    public partial class AddBlob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovementDocumentId",
                table: "Compensations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Blobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LotEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blobs_Lots_LotEntityId",
                        column: x => x.LotEntityId,
                        principalTable: "Lots",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Compensations_ApprovementDocumentId",
                table: "Compensations",
                column: "ApprovementDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Blobs_LotEntityId",
                table: "Blobs",
                column: "LotEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Compensations_Blobs_ApprovementDocumentId",
                table: "Compensations",
                column: "ApprovementDocumentId",
                principalTable: "Blobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compensations_Blobs_ApprovementDocumentId",
                table: "Compensations");

            migrationBuilder.DropTable(
                name: "Blobs");

            migrationBuilder.DropIndex(
                name: "IX_Compensations_ApprovementDocumentId",
                table: "Compensations");

            migrationBuilder.DropColumn(
                name: "ApprovementDocumentId",
                table: "Compensations");

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.CheckConstraint("CK_Images_UrlOrData", "[Url] IS NOT NULL OR [Data] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Images_Lots_LotId",
                        column: x => x.LotId,
                        principalTable: "Lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Images_LotId",
                table: "Images",
                column: "LotId");
        }
    }
}
