using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmbExplorerCompanion.Database.Migrations
{
    /// <inheritdoc />
    public partial class PlayerAwardOmitFromGroupings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OmitFromGroupings",
                table: "PlayerAwards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OmitFromGroupings",
                table: "PlayerAwards");
        }
    }
}
