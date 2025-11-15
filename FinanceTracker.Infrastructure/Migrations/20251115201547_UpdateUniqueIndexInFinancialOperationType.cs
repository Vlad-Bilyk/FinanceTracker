using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUniqueIndexInFinancialOperationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinancialOperationTypes_Kind_Name",
                table: "FinancialOperationTypes");

            migrationBuilder.DropIndex(
                name: "IX_FinancialOperationTypes_UserId",
                table: "FinancialOperationTypes");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOperationTypes_UserId_Kind_Name",
                table: "FinancialOperationTypes",
                columns: new[] { "UserId", "Kind", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinancialOperationTypes_UserId_Kind_Name",
                table: "FinancialOperationTypes");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOperationTypes_Kind_Name",
                table: "FinancialOperationTypes",
                columns: new[] { "Kind", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOperationTypes_UserId",
                table: "FinancialOperationTypes",
                column: "UserId");
        }
    }
}
