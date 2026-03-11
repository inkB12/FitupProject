using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitupProject.DAL.Migrations
{
    /// <inheritdoc />
    public partial class WorkoutSessionExcerciseFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDone",
                table: "WorkoutSessionExercises",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDone",
                table: "WorkoutSessionExercises");
        }
    }
}
