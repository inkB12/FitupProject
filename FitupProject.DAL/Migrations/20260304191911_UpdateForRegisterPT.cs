using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitupProject.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateForRegisterPT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CertificationsJson",
                table: "PTs",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "ExperienceYears",
                table: "PTs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HourlyPointRate",
                table: "PTs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LanguagesJson",
                table: "PTs",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "PTs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedReason",
                table: "PTs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewedAt",
                table: "PTs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedBy",
                table: "PTs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialtiesJson",
                table: "PTs",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SubmittedAt",
                table: "PTs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateTable(
                name: "PTCertificationFiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PTId = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleteAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PTCertificationFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PTCertificationFiles_PTs_PTId",
                        column: x => x.PTId,
                        principalTable: "PTs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PTReviewLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PTId = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    ActorAccountId = table.Column<string>(type: "text", nullable: false),
                    ActionAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SnapshotJson = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleteAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PTReviewLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PTReviewLogs_PTs_PTId",
                        column: x => x.PTId,
                        principalTable: "PTs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PTCertificationFiles_PTId",
                table: "PTCertificationFiles",
                column: "PTId");

            migrationBuilder.CreateIndex(
                name: "IX_PTReviewLogs_PTId",
                table: "PTReviewLogs",
                column: "PTId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PTCertificationFiles");

            migrationBuilder.DropTable(
                name: "PTReviewLogs");

            migrationBuilder.DropColumn(
                name: "CertificationsJson",
                table: "PTs");

            migrationBuilder.DropColumn(
                name: "ExperienceYears",
                table: "PTs");

            migrationBuilder.DropColumn(
                name: "HourlyPointRate",
                table: "PTs");

            migrationBuilder.DropColumn(
                name: "LanguagesJson",
                table: "PTs");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "PTs");

            migrationBuilder.DropColumn(
                name: "RejectedReason",
                table: "PTs");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "PTs");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "PTs");

            migrationBuilder.DropColumn(
                name: "SpecialtiesJson",
                table: "PTs");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "PTs");
        }
    }
}
