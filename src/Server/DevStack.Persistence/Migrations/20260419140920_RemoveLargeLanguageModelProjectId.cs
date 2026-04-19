using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevStack.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLargeLanguageModelProjectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LargeLanguageModels_Projects_Id",
                table: "LargeLanguageModels");

            migrationBuilder.DropForeignKey(
                name: "FK_LargeLanguageModels_Projects_ProjectId",
                table: "LargeLanguageModels");

            migrationBuilder.DropIndex(
                name: "IX_LargeLanguageModels_ProjectId",
                table: "LargeLanguageModels");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "LargeLanguageModels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "LargeLanguageModels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_LargeLanguageModels_ProjectId",
                table: "LargeLanguageModels",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_LargeLanguageModels_Projects_Id",
                table: "LargeLanguageModels",
                column: "Id",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LargeLanguageModels_Projects_ProjectId",
                table: "LargeLanguageModels",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
