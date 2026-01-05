using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PTJ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAIChatEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIChatSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIChatSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIChatSessions_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIChatMessages_AIChatSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AIChatSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 4, 3, 56, 23, 141, DateTimeKind.Utc).AddTicks(1843));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 4, 3, 56, 23, 141, DateTimeKind.Utc).AddTicks(2298));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 4, 3, 56, 23, 141, DateTimeKind.Utc).AddTicks(2299));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 4, 3, 56, 23, 141, DateTimeKind.Utc).AddTicks(2301));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 4, 3, 56, 23, 141, DateTimeKind.Utc).AddTicks(2302));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 4, 3, 56, 23, 141, DateTimeKind.Utc).AddTicks(2304));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 4, 3, 56, 23, 141, DateTimeKind.Utc).AddTicks(2305));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 4, 3, 56, 23, 141, DateTimeKind.Utc).AddTicks(2306));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 4, 3, 56, 23, 141, DateTimeKind.Utc).AddTicks(2308));

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 4, 3, 56, 23, 168, DateTimeKind.Utc).AddTicks(2379));

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 4, 3, 56, 23, 168, DateTimeKind.Utc).AddTicks(2385));

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 4, 3, 56, 23, 168, DateTimeKind.Utc).AddTicks(2387));

            migrationBuilder.CreateIndex(
                name: "IX_AIChatMessages_SessionId",
                table: "AIChatMessages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AIChatSessions_UserId",
                table: "AIChatSessions",
                column: "UserId");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIChatMessages");



            migrationBuilder.DropTable(
                name: "AIChatSessions");



            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 7, 19, 21, 684, DateTimeKind.Utc).AddTicks(808));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 7, 19, 21, 684, DateTimeKind.Utc).AddTicks(2048));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 7, 19, 21, 684, DateTimeKind.Utc).AddTicks(2052));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 7, 19, 21, 684, DateTimeKind.Utc).AddTicks(2056));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 7, 19, 21, 684, DateTimeKind.Utc).AddTicks(2059));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 7, 19, 21, 684, DateTimeKind.Utc).AddTicks(2063));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 7, 19, 21, 684, DateTimeKind.Utc).AddTicks(2066));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 7, 19, 21, 684, DateTimeKind.Utc).AddTicks(2070));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 7, 19, 21, 684, DateTimeKind.Utc).AddTicks(2073));

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 7, 19, 21, 737, DateTimeKind.Utc).AddTicks(2727));

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 7, 19, 21, 737, DateTimeKind.Utc).AddTicks(2736));

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 7, 19, 21, 737, DateTimeKind.Utc).AddTicks(2739));
        }
    }
}
