using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UAMS.Room.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoomDesign");

            migrationBuilder.CreateTable(
                name: "AssetDefinitions",
                schema: "RoomDesign",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    SvgUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AssetAllowedLocations = table.Column<int[]>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlacedAssetChecklists",
                schema: "RoomDesign",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlacedAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyYear = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacedAssetChecklists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                schema: "RoomDesign",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BuildingId = table.Column<Guid>(type: "uuid", nullable: false),
                    FacultyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ClosureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DesignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Layout = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                schema: "RoomDesign",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlacedAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    FacultyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaintainerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfirmedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistItemTemplates",
                schema: "RoomDesign",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItemTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistItemTemplates_AssetDefinitions_AssetDefinitionId",
                        column: x => x.AssetDefinitionId,
                        principalSchema: "RoomDesign",
                        principalTable: "AssetDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketNotes",
                schema: "RoomDesign",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketNotes_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalSchema: "RoomDesign",
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlacedAssetChecklistEntries",
                schema: "RoomDesign",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetChecklistId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChecklistItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsChecked = table.Column<bool>(type: "boolean", nullable: false),
                    CheckedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacedAssetChecklistEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlacedAssetChecklistEntries_ChecklistItemTemplates_Checklis~",
                        column: x => x.ChecklistItemId,
                        principalSchema: "RoomDesign",
                        principalTable: "ChecklistItemTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlacedAssetChecklistEntries_PlacedAssetChecklists_AssetChec~",
                        column: x => x.AssetChecklistId,
                        principalSchema: "RoomDesign",
                        principalTable: "PlacedAssetChecklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItemTemplates_AssetDefinitionId",
                schema: "RoomDesign",
                table: "ChecklistItemTemplates",
                column: "AssetDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacedAssetChecklistEntries_AssetChecklistId",
                schema: "RoomDesign",
                table: "PlacedAssetChecklistEntries",
                column: "AssetChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacedAssetChecklistEntries_ChecklistItemId",
                schema: "RoomDesign",
                table: "PlacedAssetChecklistEntries",
                column: "ChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_FacultyId",
                schema: "RoomDesign",
                table: "Rooms",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketNotes_TicketId",
                schema: "RoomDesign",
                table: "TicketNotes",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_DepartmentId",
                schema: "RoomDesign",
                table: "Tickets",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_FacultyId",
                schema: "RoomDesign",
                table: "Tickets",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_PlacedAssetId",
                schema: "RoomDesign",
                table: "Tickets",
                column: "PlacedAssetId",
                unique: true,
                filter: "\"Status\" NOT IN (8, 9, 10, 11)");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ReporterId",
                schema: "RoomDesign",
                table: "Tickets",
                column: "ReporterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlacedAssetChecklistEntries",
                schema: "RoomDesign");

            migrationBuilder.DropTable(
                name: "Rooms",
                schema: "RoomDesign");

            migrationBuilder.DropTable(
                name: "TicketNotes",
                schema: "RoomDesign");

            migrationBuilder.DropTable(
                name: "ChecklistItemTemplates",
                schema: "RoomDesign");

            migrationBuilder.DropTable(
                name: "PlacedAssetChecklists",
                schema: "RoomDesign");

            migrationBuilder.DropTable(
                name: "Tickets",
                schema: "RoomDesign");

            migrationBuilder.DropTable(
                name: "AssetDefinitions",
                schema: "RoomDesign");
        }
    }
}
