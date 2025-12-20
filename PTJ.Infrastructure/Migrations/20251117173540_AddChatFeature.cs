using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PTJ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "chat");

            migrationBuilder.CreateTable(
                name: "ChatConversations",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployerId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    JobPostId = table.Column<int>(type: "int", nullable: true),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsEmployerTyping = table.Column<bool>(type: "bit", nullable: false),
                    IsStudentTyping = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ChatConversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatConversations_JobPosts_JobPostId",
                        column: x => x.JobPostId,
                        principalSchema: "jobs",
                        principalTable: "JobPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChatConversations_Users_EmployerId",
                        column: x => x.EmployerId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatConversations_Users_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalSchema: "chat",
                        principalTable: "ChatConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Users_SenderId",
                        column: x => x.SenderId,
                        principalSchema: "auth",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 17, 35, 39, 180, DateTimeKind.Utc).AddTicks(395));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 17, 35, 39, 180, DateTimeKind.Utc).AddTicks(1283));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 17, 35, 39, 180, DateTimeKind.Utc).AddTicks(1285));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 17, 35, 39, 180, DateTimeKind.Utc).AddTicks(1287));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 17, 35, 39, 180, DateTimeKind.Utc).AddTicks(1291));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 17, 35, 39, 180, DateTimeKind.Utc).AddTicks(1293));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 17, 35, 39, 180, DateTimeKind.Utc).AddTicks(1295));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 17, 35, 39, 180, DateTimeKind.Utc).AddTicks(1296));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 17, 35, 39, 180, DateTimeKind.Utc).AddTicks(1298));

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 17, 35, 39, 233, DateTimeKind.Utc).AddTicks(984));

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 17, 35, 39, 233, DateTimeKind.Utc).AddTicks(991));

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 17, 35, 39, 233, DateTimeKind.Utc).AddTicks(993));

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversations_EmployerId_StudentId",
                schema: "chat",
                table: "ChatConversations",
                columns: new[] { "EmployerId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversations_JobPostId",
                schema: "chat",
                table: "ChatConversations",
                column: "JobPostId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversations_LastMessageAt",
                schema: "chat",
                table: "ChatConversations",
                column: "LastMessageAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversations_StudentId",
                schema: "chat",
                table: "ChatConversations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ConversationId",
                schema: "chat",
                table: "ChatMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ConversationId_IsRead",
                schema: "chat",
                table: "ChatMessages",
                columns: new[] { "ConversationId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_CreatedAt",
                schema: "chat",
                table: "ChatMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId",
                schema: "chat",
                table: "ChatMessages",
                column: "SenderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "ChatConversations",
                schema: "chat");

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 6, 16, 44, 291, DateTimeKind.Utc).AddTicks(4812));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 6, 16, 44, 291, DateTimeKind.Utc).AddTicks(5239));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 6, 16, 44, 291, DateTimeKind.Utc).AddTicks(5241));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 6, 16, 44, 291, DateTimeKind.Utc).AddTicks(5242));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 6, 16, 44, 291, DateTimeKind.Utc).AddTicks(5244));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 6, 16, 44, 291, DateTimeKind.Utc).AddTicks(5245));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 6, 16, 44, 291, DateTimeKind.Utc).AddTicks(5247));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 6, 16, 44, 291, DateTimeKind.Utc).AddTicks(5248));

            migrationBuilder.UpdateData(
                schema: "jobs",
                table: "ApplicationStatuses",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 6, 16, 44, 291, DateTimeKind.Utc).AddTicks(5250));

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 6, 16, 44, 327, DateTimeKind.Utc).AddTicks(3043));

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 6, 16, 44, 327, DateTimeKind.Utc).AddTicks(3048));

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 6, 16, 44, 327, DateTimeKind.Utc).AddTicks(3050));
        }
    }
}
