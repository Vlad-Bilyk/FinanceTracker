using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAndWalletEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "FinancialOperations");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountBase",
                table: "FinancialOperations",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyOriginal",
                table: "FinancialOperations",
                type: "character(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WalletId",
                table: "FinancialOperations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaseCurrencyCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOperations_WalletId",
                table: "FinancialOperations",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId_Name",
                table: "Wallets",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialOperations_Wallets_WalletId",
                table: "FinancialOperations",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialOperations_Wallets_WalletId",
                table: "FinancialOperations");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_FinancialOperations_WalletId",
                table: "FinancialOperations");

            migrationBuilder.DropColumn(
                name: "AmountBase",
                table: "FinancialOperations");

            migrationBuilder.DropColumn(
                name: "CurrencyOriginal",
                table: "FinancialOperations");

            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "FinancialOperations");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "FinancialOperations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
