using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioTracker.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAssetModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Assets");

            migrationBuilder.RenameColumn(
                name: "PurchasePrice",
                table: "Assets",
                newName: "CurrentValue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrentValue",
                table: "Assets",
                newName: "PurchasePrice");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Assets",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
