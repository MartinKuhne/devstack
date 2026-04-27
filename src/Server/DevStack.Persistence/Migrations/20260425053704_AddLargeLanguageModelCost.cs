using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevStack.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLargeLanguageModelCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Cost",
                table: "LargeLanguageModels",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cost",
                table: "LargeLanguageModels");
        }
    }
}
