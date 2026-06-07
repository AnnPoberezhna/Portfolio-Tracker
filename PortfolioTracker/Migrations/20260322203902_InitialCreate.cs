using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioTracker.Migrations
{
    /// <summary>
    /// Inicjalna migracja tworząca początkową strukturę bazy danych dla aplikacji PortfolioTracker.
    /// Odpowiada za wygenerowanie głównej tabeli przechowującej aktywa użytkownika.
    /// </summary>
    public partial class InitialCreate : Migration
    {
        /// <summary>
        /// Aplikuje migrację do bazy danych. 
        /// Tworzy tabelę "Assets" zawierającą podstawowe informacje o aktywach: identyfikator, nazwę, symbol, 
        /// posiadaną ilość, cenę zakupu oraz datę transakcji.
        /// </summary>
        /// <param name="migrationBuilder">Obiekt dostarczany przez EF Core, służący do definiowania operacji na strukturze bazy danych.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });
        }

        /// <summary>
        /// Wycofuje migrację (odwraca zmiany dokonane w metodzie Up).
        /// Całkowicie usuwa tabelę "Assets" ze schematu bazy danych.
        /// </summary>
        /// <param name="migrationBuilder">Obiekt dostarczany przez EF Core, służący do definiowania operacji na strukturze bazy danych.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");
        }
    }
}
