using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteIsDeletedColumnFromFinancialOperationTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinancialOperationTypes_UserId_Kind_Name_IsDeleted",
                table: "FinancialOperationTypes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
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

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FinancialOperationTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOperationTypes_UserId_Kind_Name_IsDeleted",
                table: "FinancialOperationTypes",
                columns: new[] { "UserId", "Kind", "Name", "IsDeleted" },
                unique: true);
        }
    }
}
