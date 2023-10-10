using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmbExplorerCompanion.Database.Migrations
{
    /// <inheritdoc />
    public partial class PlayerAwardFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBattingAward",
                table: "PlayerAwards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFieldingAward",
                table: "PlayerAwards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPitchingAward",
                table: "PlayerAwards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPlayoffAward",
                table: "PlayerAwards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUserAssignable",
                table: "PlayerAwards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBattingAward",
                table: "PlayerAwards");

            migrationBuilder.DropColumn(
                name: "IsFieldingAward",
                table: "PlayerAwards");

            migrationBuilder.DropColumn(
                name: "IsPitchingAward",
                table: "PlayerAwards");

            migrationBuilder.DropColumn(
                name: "IsPlayoffAward",
                table: "PlayerAwards");

            migrationBuilder.DropColumn(
                name: "IsUserAssignable",
                table: "PlayerAwards");
        }
    }
}
