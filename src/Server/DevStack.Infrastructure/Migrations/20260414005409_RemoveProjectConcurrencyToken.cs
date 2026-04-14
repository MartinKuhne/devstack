using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevStack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProjectConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                table: "Projects");

            migrationBuilder.AddColumn<Guid>(
                name: "FeatureId1",
                table: "Tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_FeatureId1",
                table: "Tasks",
                column: "FeatureId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Features_FeatureId1",
                table: "Tasks",
                column: "FeatureId1",
                principalTable: "Features",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Features_FeatureId1",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_FeatureId1",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "FeatureId1",
                table: "Tasks");

            migrationBuilder.AddColumn<byte[]>(
                name: "ConcurrencyToken",
                table: "Projects",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
