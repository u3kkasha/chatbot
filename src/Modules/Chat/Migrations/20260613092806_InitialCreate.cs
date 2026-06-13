using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Chatbot.Modules.Chat.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "chat");

            migrationBuilder.CreateTable(
                name: "chat_messages",
                schema: "chat",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    is_ai_generated = table.Column<bool>(type: "boolean", nullable: false),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_date = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ai_metadata = table.Column<string>(type: "jsonb", nullable: true),
                    citations = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chat_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chat_sessions",
                schema: "chat",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel_provider = table.Column<string>(type: "text", nullable: false),
                    external_reference_id = table.Column<string>(type: "text", nullable: true),
                    customer_identifier = table.Column<string>(type: "text", nullable: false),
                    operator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chat_sessions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_messages_session",
                schema: "chat",
                table: "chat_messages",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "idx_sessions_channel",
                schema: "chat",
                table: "chat_sessions",
                columns: new[] { "channel_provider", "external_reference_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chat_messages",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "chat_sessions",
                schema: "chat");
        }
    }
}
