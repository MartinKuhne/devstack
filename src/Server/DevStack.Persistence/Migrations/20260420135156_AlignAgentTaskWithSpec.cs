using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevStack.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignAgentTaskWithSpec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DependsOnDevTask",
                table: "AgentTasks");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "AgentTasks");

            migrationBuilder.AddColumn<string>(
                name: "Agent",
                table: "AgentTasks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DependsOnAgentTaskId",
                table: "AgentTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AgentTasks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AgentTasks_DependsOnAgentTaskId",
                table: "AgentTasks",
                column: "DependsOnAgentTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentTasks_AgentTasks_DependsOnAgentTaskId",
                table: "AgentTasks",
                column: "DependsOnAgentTaskId",
                principalTable: "AgentTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentTasks_AgentTasks_DependsOnAgentTaskId",
                table: "AgentTasks");

            migrationBuilder.DropIndex(
                name: "IX_AgentTasks_DependsOnAgentTaskId",
                table: "AgentTasks");

            migrationBuilder.DropColumn(
                name: "Agent",
                table: "AgentTasks");

            migrationBuilder.DropColumn(
                name: "DependsOnAgentTaskId",
                table: "AgentTasks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AgentTasks");

            migrationBuilder.AddColumn<string>(
                name: "DependsOnDevTask",
                table: "AgentTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "AgentTasks",
                type: "text",
                nullable: true);
        }
    }
}
