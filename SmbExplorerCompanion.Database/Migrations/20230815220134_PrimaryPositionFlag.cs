using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmbExplorerCompanion.Database.Migrations
{
    /// <inheritdoc />
    public partial class PrimaryPositionFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrimaryPosition",
                table: "Positions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrimaryPosition",
                table: "Positions");
        }
    }
}
