using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevStack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDependsOnToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DependsOnId",
                table: "Items",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_DependsOnId",
                table: "Items",
                column: "DependsOnId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Items_DependsOnId",
                table: "Items",
                column: "DependsOnId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Items_DependsOnId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_DependsOnId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DependsOnId",
                table: "Items");
        }
    }
}
