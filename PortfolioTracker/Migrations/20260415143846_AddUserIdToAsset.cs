using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioTracker.Migrations
{
    /// <summary>
    /// Migracja wprowadzająca relację między aktywami a użytkownikami systemu.
    /// Odpowiada za dodanie klucza obcego (UserId) do tabeli aktywów, co pozwala na przypisanie 
    /// konkretnego aktywa do konkretnego właściciela (konta użytkownika).
    /// </summary>
    public partial class AddUserIdToAsset : Migration
    {
        /// <summary>
        /// Aplikuje migrację do bazy danych. 
        /// Dodaje wymaganą kolumnę "UserId" do tabeli "Assets", zakłada na niej indeks optymalizujący wyszukiwanie 
        /// oraz tworzy relację klucza obcego do tabeli "AspNetUsers" (z włączonym usuwaniem kaskadowym).
        /// </summary>
        /// <param name="migrationBuilder">Obiekt dostarczany przez EF Core, służący do definiowania operacji na strukturze bazy danych.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Assets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_UserId",
                table: "Assets",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_AspNetUsers_UserId",
                table: "Assets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <summary>
        /// Wycofuje migrację (odwraca zmiany dokonane w metodzie Up).
        /// Usuwa relację klucza obcego, usuwa indeks "IX_Assets_UserId", a ostatecznie kasuje 
        /// całą kolumnę "UserId" z tabeli "Assets", zrywając powiązanie aktywów z użytkownikami.
        /// </summary>
        /// <param name="migrationBuilder">Obiekt dostarczany przez EF Core, służący do definiowania operacji na strukturze bazy danych.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_AspNetUsers_UserId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_UserId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Assets");
        }
    }
}
