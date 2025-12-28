using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitupProject.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddWeeksToOnboardingProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Weeks",
                table: "OnboardingProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Weeks",
                table: "OnboardingProfiles");
        }
    }
}
