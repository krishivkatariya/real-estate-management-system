using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace realEstate.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Amenities",
                table: "Properties",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Balconies",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Furnishing",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pincode",
                table: "Properties",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Properties",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "Properties",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.CreateTable(
                name: "BookingRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    SellerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PreferredDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PreferredTime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SellerResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingRequests_AspNetUsers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingRequests_AspNetUsers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingRequests_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Properties_ApprovalStatus",
                table: "Properties",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_BuyerId",
                table: "BookingRequests",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_PropertyId_BuyerId",
                table: "BookingRequests",
                columns: new[] { "PropertyId", "BuyerId" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_SellerId",
                table: "BookingRequests",
                column: "SellerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingRequests");

            migrationBuilder.DropIndex(
                name: "IX_Properties_ApprovalStatus",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Amenities",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Balconies",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Furnishing",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Pincode",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Properties");
        }
    }
}
