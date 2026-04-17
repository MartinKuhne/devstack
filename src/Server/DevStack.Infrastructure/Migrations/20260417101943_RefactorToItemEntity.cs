using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevStack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorToItemEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Items_ItemId1",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ItemId1",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ItemId1",
                table: "Tasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ItemId1",
                table: "Tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ItemId1",
                table: "Tasks",
                column: "ItemId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Items_ItemId1",
                table: "Tasks",
                column: "ItemId1",
                principalTable: "Items",
                principalColumn: "Id");
        }
    }
}
