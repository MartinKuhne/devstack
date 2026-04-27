using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevStack.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignWithGraphQLSpec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "LargeLanguageModels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "ExecutionDurationInSeconds",
                table: "AgentTasks",
                type: "integer",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LargeLanguageModels_ProjectId",
                table: "LargeLanguageModels",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_LargeLanguageModels_Projects_ProjectId",
                table: "LargeLanguageModels",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LargeLanguageModels_Projects_ProjectId",
                table: "LargeLanguageModels");

            migrationBuilder.DropIndex(
                name: "IX_LargeLanguageModels_ProjectId",
                table: "LargeLanguageModels");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "LargeLanguageModels");

            migrationBuilder.AlterColumn<double>(
                name: "ExecutionDurationInSeconds",
                table: "AgentTasks",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
