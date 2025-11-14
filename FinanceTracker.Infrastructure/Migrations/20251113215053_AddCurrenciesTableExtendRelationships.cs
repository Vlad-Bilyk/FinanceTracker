using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrenciesTableExtendRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrencyOriginal",
                table: "FinancialOperations",
                newName: "CurrencyOriginalCode");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "FinancialOperationTypes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "AmountOriginal",
                table: "FinancialOperations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Code);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_BaseCurrencyCode",
                table: "Wallets",
                column: "BaseCurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOperationTypes_UserId",
                table: "FinancialOperationTypes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOperations_CurrencyOriginalCode",
                table: "FinancialOperations",
                column: "CurrencyOriginalCode");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialOperations_Currencies_CurrencyOriginalCode",
                table: "FinancialOperations",
                column: "CurrencyOriginalCode",
                principalTable: "Currencies",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialOperationTypes_Users_UserId",
                table: "FinancialOperationTypes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Currencies_BaseCurrencyCode",
                table: "Wallets",
                column: "BaseCurrencyCode",
                principalTable: "Currencies",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialOperations_Currencies_CurrencyOriginalCode",
                table: "FinancialOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialOperationTypes_Users_UserId",
                table: "FinancialOperationTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Currencies_BaseCurrencyCode",
                table: "Wallets");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_BaseCurrencyCode",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_FinancialOperationTypes_UserId",
                table: "FinancialOperationTypes");

            migrationBuilder.DropIndex(
                name: "IX_FinancialOperations_CurrencyOriginalCode",
                table: "FinancialOperations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FinancialOperationTypes");

            migrationBuilder.DropColumn(
                name: "AmountOriginal",
                table: "FinancialOperations");

            migrationBuilder.RenameColumn(
                name: "CurrencyOriginalCode",
                table: "FinancialOperations",
                newName: "CurrencyOriginal");
        }
    }
}
