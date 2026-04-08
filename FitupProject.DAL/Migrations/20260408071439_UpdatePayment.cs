using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitupProject.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_VnpTxnRef",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VnpBankCode",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VnpResponseCode",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VnpTransactionNo",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "VnpTxnRef",
                table: "Payments",
                newName: "ProviderTransactionId");

            migrationBuilder.RenameColumn(
                name: "VnpTransactionStatus",
                table: "Payments",
                newName: "ConfirmedBy");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountName",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNo",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankCode",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ConfirmedAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Method",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TransferContent",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccountName",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BankAccountNo",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BankCode",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TransferContent",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "ProviderTransactionId",
                table: "Payments",
                newName: "VnpTxnRef");

            migrationBuilder.RenameColumn(
                name: "ConfirmedBy",
                table: "Payments",
                newName: "VnpTransactionStatus");

            migrationBuilder.AddColumn<string>(
                name: "VnpBankCode",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VnpResponseCode",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VnpTransactionNo",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_VnpTxnRef",
                table: "Payments",
                column: "VnpTxnRef",
                unique: true);
        }
    }
}
