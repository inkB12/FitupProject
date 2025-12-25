using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitupProject.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailOtpExpiresAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmailOtpFailCount",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EmailOtpHash",
                table: "Accounts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailVerifiedAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailOtpExpiresAt",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "EmailOtpFailCount",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "EmailOtpHash",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "EmailVerifiedAt",
                table: "Accounts");
        }
    }
}
