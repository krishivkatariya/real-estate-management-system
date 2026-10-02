using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace realEstate.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalRequestConcurrencyProtection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_RentalTransactions_PendingPropertyRenter",
                table: "RentalTransactions",
                columns: new[] { "PropertyId", "RenterId" },
                unique: true,
                filter: "[Status] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_RentalTransactions_PendingPropertyRenter",
                table: "RentalTransactions");
        }
    }
}
