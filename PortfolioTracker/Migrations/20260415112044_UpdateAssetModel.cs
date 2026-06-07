using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioTracker.Migrations
{
    /// <summary>
    /// Migracja aktualizująca model danych dla aktywów (Assets).
    /// Odpowiada za usunięcie zbędnej kolumny z nazwą oraz zmianę przeznaczenia kolumny z ceną.
    /// </summary>
    public partial class UpdateAssetModel : Migration
    {
        /// <summary>
        /// Aplikuje migrację do bazy danych. 
        /// Trwale usuwa kolumnę "Name" z tabeli "Assets" oraz zmienia nazwę kolumny z "PurchasePrice" na "CurrentValue".
        /// </summary>
        /// <param name="migrationBuilder">Obiekt dostarczany przez EF Core, służący do definiowania operacji na strukturze bazy danych.</param>
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

        /// <summary>
            /// Wycofuje migrację (odwraca zmiany dokonane w metodzie Up).
            /// Odtwarza kolumnę "Name" o maksymalnej długości 50 znaków i przywraca pierwotną nazwę kolumny "PurchasePrice".
            /// </summary>
            /// <param name="migrationBuilder">Obiekt dostarczany przez EF Core, służący do definiowania operacji na strukturze bazy danych.</param>
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
