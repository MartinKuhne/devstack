using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevStack.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliverableDesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Design",
                table: "Deliverables",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Design",
                table: "Deliverables");
        }
    }
}
