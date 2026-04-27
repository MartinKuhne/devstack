using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevStack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEpicEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EpicId",
                table: "Features",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Epics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Epics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Features_EpicId",
                table: "Features",
                column: "EpicId");

            migrationBuilder.CreateIndex(
                name: "IX_Epics_Title",
                table: "Epics",
                column: "Title");

            migrationBuilder.AddForeignKey(
                name: "FK_Features_Epics_EpicId",
                table: "Features",
                column: "EpicId",
                principalTable: "Epics",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Features_Epics_EpicId",
                table: "Features");

            migrationBuilder.DropTable(
                name: "Epics");

            migrationBuilder.DropIndex(
                name: "IX_Features_EpicId",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "EpicId",
                table: "Features");
        }
    }
}
