using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitupProject.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddResetPasswordOtpFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ResetPasswordOtpExpiresAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResetPasswordOtpFailCount",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ResetPasswordOtpHash",
                table: "Accounts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetPasswordOtpExpiresAt",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ResetPasswordOtpFailCount",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ResetPasswordOtpHash",
                table: "Accounts");
        }
    }
}
