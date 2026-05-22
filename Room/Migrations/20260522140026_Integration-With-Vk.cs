using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UAMS.Room.Migrations
{
    /// <inheritdoc />
    public partial class IntegrationWithVk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedMaintainerVkId",
                schema: "RoomDesign",
                table: "Tickets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VkNotificationStatus",
                schema: "RoomDesign",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedMaintainerVkId",
                schema: "RoomDesign",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "VkNotificationStatus",
                schema: "RoomDesign",
                table: "Tickets");
        }
    }
}
