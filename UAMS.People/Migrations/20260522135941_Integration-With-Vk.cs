using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UAMS.Campus.Migrations
{
    /// <inheritdoc />
    public partial class IntegrationWithVk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VkId",
                schema: "campus",
                table: "maintainers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VkId",
                schema: "campus",
                table: "maintainers");
        }
    }
}
