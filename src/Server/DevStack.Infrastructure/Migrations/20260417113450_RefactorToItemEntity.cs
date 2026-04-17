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
                name: "FK_Tasks_Features_FeatureId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Features_FeatureId1",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowRuns_Features_FeatureId",
                table: "WorkflowRuns");

            migrationBuilder.DropTable(
                name: "Defects");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_FeatureId1",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "FeatureId1",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "FeatureId",
                table: "WorkflowRuns",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowRuns_FeatureId",
                table: "WorkflowRuns",
                newName: "IX_WorkflowRuns_ItemId");

            migrationBuilder.RenameColumn(
                name: "FeatureId",
                table: "Tasks",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_FeatureId_Status",
                table: "Tasks",
                newName: "IX_Tasks_ItemId_Status");

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Subtype = table.Column<int>(type: "integer", nullable: false),
                    EpicId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentFeatureId = table.Column<Guid>(type: "uuid", nullable: true),
                    Severity = table.Column<int>(type: "integer", nullable: true),
                    RootCause = table.Column<string>(type: "text", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AcceptanceCriteria = table.Column<string>(type: "text", nullable: true),
                    Plan = table.Column<string>(type: "text", nullable: true),
                    SecurityImpact = table.Column<string>(type: "text", nullable: true),
                    PerformanceImpact = table.Column<string>(type: "text", nullable: true),
                    TestPlan = table.Column<string>(type: "text", nullable: true),
                    DeploymentPlan = table.Column<string>(type: "text", nullable: true),
                    OpenQuestions = table.Column<string>(type: "text", nullable: true),
                    Result = table.Column<string>(type: "text", nullable: true),
                    Errors = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Epics_EpicId",
                        column: x => x.EpicId,
                        principalTable: "Epics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Items_Items_ParentFeatureId",
                        column: x => x.ParentFeatureId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Items_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_EpicId",
                table: "Items",
                column: "EpicId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ParentFeatureId",
                table: "Items",
                column: "ParentFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ProjectId_Status",
                table: "Items",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_Status",
                table: "Items",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Subtype",
                table: "Items",
                column: "Subtype");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Items_ItemId",
                table: "Tasks",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowRuns_Items_ItemId",
                table: "WorkflowRuns",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Items_ItemId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowRuns_Items_ItemId",
                table: "WorkflowRuns");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "WorkflowRuns",
                newName: "FeatureId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowRuns_ItemId",
                table: "WorkflowRuns",
                newName: "IX_WorkflowRuns_FeatureId");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "Tasks",
                newName: "FeatureId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_ItemId_Status",
                table: "Tasks",
                newName: "IX_Tasks_FeatureId_Status");

            migrationBuilder.AddColumn<Guid>(
                name: "FeatureId1",
                table: "Tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpicId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcceptanceCriteria = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeploymentPlan = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Errors = table.Column<string>(type: "text", nullable: true),
                    OpenQuestions = table.Column<string>(type: "text", nullable: true),
                    PerformanceImpact = table.Column<string>(type: "text", nullable: true),
                    Plan = table.Column<string>(type: "text", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: true),
                    SecurityImpact = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TestPlan = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Features_Epics_EpicId",
                        column: x => x.EpicId,
                        principalTable: "Epics",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Features_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Defects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentFeatureId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcceptanceCriteria = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeploymentPlan = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Errors = table.Column<string>(type: "text", nullable: true),
                    OpenQuestions = table.Column<string>(type: "text", nullable: true),
                    PerformanceImpact = table.Column<string>(type: "text", nullable: true),
                    Plan = table.Column<string>(type: "text", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: true),
                    RootCause = table.Column<string>(type: "text", nullable: true),
                    SecurityImpact = table.Column<string>(type: "text", nullable: true),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TestPlan = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Defects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Defects_Features_ParentFeatureId",
                        column: x => x.ParentFeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Defects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_FeatureId1",
                table: "Tasks",
                column: "FeatureId1");

            migrationBuilder.CreateIndex(
                name: "IX_Defects_ParentFeatureId",
                table: "Defects",
                column: "ParentFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_Defects_ProjectId_Status",
                table: "Defects",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Features_EpicId",
                table: "Features",
                column: "EpicId");

            migrationBuilder.CreateIndex(
                name: "IX_Features_ProjectId_Status",
                table: "Features",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Features_Status",
                table: "Features",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Features_FeatureId",
                table: "Tasks",
                column: "FeatureId",
                principalTable: "Features",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Features_FeatureId1",
                table: "Tasks",
                column: "FeatureId1",
                principalTable: "Features",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowRuns_Features_FeatureId",
                table: "WorkflowRuns",
                column: "FeatureId",
                principalTable: "Features",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
