using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioTracker.Migrations
{
    /// <summary>
    /// Migracja wprowadzająca funkcjonalność śledzenia aktywności użytkowników.
    /// Tworzy nową tabelę przeznaczoną do przechowywania metadanych konta, takich jak data rejestracji i czas ostatniej aktywności.
    /// </summary>
    public partial class AddUserActivityTracking : Migration
    {
        /// <summary>
        /// Aplikuje migrację do bazy danych.
        /// Tworzy tabelę "UserActivities" z kolumnami "RegisteredAtUtc" oraz "LastSeenUtc". 
        /// Ustanawia "UserId" jako klucz główny, będący jednocześnie kluczem obcym powiązanym kaskadowo z tabelą "AspNetUsers".
        /// </summary>
        /// <param name="migrationBuilder">Obiekt dostarczany przez EF Core, służący do definiowania operacji na strukturze bazy danych.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserActivities",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RegisteredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSeenUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivities", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserActivities_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <summary>
        /// Wycofuje migrację (odwraca zmiany dokonane w metodzie Up).
        /// Trwale usuwa tabelę "UserActivities" oraz jej powiązania z bazy danych.
        /// </summary>
        /// <param name="migrationBuilder">Obiekt dostarczany przez EF Core, służący do definiowania operacji na strukturze bazy danych.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserActivities");
        }
    }
}
