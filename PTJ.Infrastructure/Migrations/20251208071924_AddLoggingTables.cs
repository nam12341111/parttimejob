using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PTJ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLoggingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "logging");

            migrationBuilder.CreateTable(
                name: "SystemErrorLogs",
                schema: "logging",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ExceptionType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InnerException = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    RequestPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemErrorLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemErrorLogs_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserActivityLogs",
                schema: "logging",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    QueryString = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivityLogs_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_SystemErrorLogs_Level",
                schema: "logging",
                table: "SystemErrorLogs",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_SystemErrorLogs_Level_Timestamp",
                schema: "logging",
                table: "SystemErrorLogs",
                columns: new[] { "Level", "Timestamp" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_SystemErrorLogs_Timestamp",
                schema: "logging",
                table: "SystemErrorLogs",
                column: "Timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_SystemErrorLogs_UserId",
                schema: "logging",
                table: "SystemErrorLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_Timestamp",
                schema: "logging",
                table: "UserActivityLogs",
                column: "Timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_UserId",
                schema: "logging",
                table: "UserActivityLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_UserId_Timestamp",
                schema: "logging",
                table: "UserActivityLogs",
                columns: new[] { "UserId", "Timestamp" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemErrorLogs",
                schema: "logging");

            migrationBuilder.DropTable(
                name: "UserActivityLogs",
                schema: "logging");

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 20, 15, 39, 43, 156, DateTimeKind.Utc).AddTicks(5162));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 20, 15, 39, 43, 156, DateTimeKind.Utc).AddTicks(5608));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 20, 15, 39, 43, 156, DateTimeKind.Utc).AddTicks(5610));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 20, 15, 39, 43, 156, DateTimeKind.Utc).AddTicks(5611));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 20, 15, 39, 43, 156, DateTimeKind.Utc).AddTicks(5613));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 20, 15, 39, 43, 156, DateTimeKind.Utc).AddTicks(5614));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 20, 15, 39, 43, 156, DateTimeKind.Utc).AddTicks(5616));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 20, 15, 39, 43, 156, DateTimeKind.Utc).AddTicks(5617));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 20, 15, 39, 43, 156, DateTimeKind.Utc).AddTicks(5619));

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 20, 15, 39, 43, 190, DateTimeKind.Utc).AddTicks(8976));

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 20, 15, 39, 43, 190, DateTimeKind.Utc).AddTicks(8985));

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 20, 15, 39, 43, 190, DateTimeKind.Utc).AddTicks(8988));
        }
    }
}
