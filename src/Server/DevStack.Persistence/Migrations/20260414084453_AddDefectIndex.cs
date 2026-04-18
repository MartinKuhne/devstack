using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevStack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefectIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Defects_ProjectId",
                table: "Defects");

            migrationBuilder.CreateIndex(
                name: "IX_Defects_ProjectId_Status",
                table: "Defects",
                columns: new[] { "ProjectId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Defects_ProjectId_Status",
                table: "Defects");

            migrationBuilder.CreateIndex(
                name: "IX_Defects_ProjectId",
                table: "Defects",
                column: "ProjectId");
        }
    }
}
