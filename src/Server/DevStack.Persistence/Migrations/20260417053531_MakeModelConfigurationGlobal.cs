using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevStack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeModelConfigurationGlobal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModelConfigurations_Projects_ProjectId",
                table: "ModelConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_ModelConfigurations_ProjectId",
                table: "ModelConfigurations");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ModelConfigurations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "ModelConfigurations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ModelConfigurations_ProjectId",
                table: "ModelConfigurations",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ModelConfigurations_Projects_ProjectId",
                table: "ModelConfigurations",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
