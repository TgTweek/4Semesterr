using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBossDifficultyAndProgressTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BossesDefeated",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DifficultyTier",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HighestDifficultyTierReached",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsBoss",
                table: "MonsterDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BossesDefeated",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "DifficultyTier",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "HighestDifficultyTierReached",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "IsBoss",
                table: "MonsterDefinitions");
        }
    }
}
