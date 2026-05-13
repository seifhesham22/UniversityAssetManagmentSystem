using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UAMS.Campus.Migrations
{
    /// <inheritdoc />
    public partial class loksdf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FacultyBuilding_buildings_BuildingId",
                schema: "campus",
                table: "FacultyBuilding");

            migrationBuilder.DropForeignKey(
                name: "FK_FacultyBuilding_faculties_FacultyId",
                schema: "campus",
                table: "FacultyBuilding");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FacultyBuilding",
                schema: "campus",
                table: "FacultyBuilding");

            migrationBuilder.RenameTable(
                name: "FacultyBuilding",
                schema: "campus",
                newName: "facultyBuildings",
                newSchema: "campus");

            migrationBuilder.RenameIndex(
                name: "IX_FacultyBuilding_BuildingId",
                schema: "campus",
                table: "facultyBuildings",
                newName: "IX_facultyBuildings_BuildingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_facultyBuildings",
                schema: "campus",
                table: "facultyBuildings",
                columns: new[] { "FacultyId", "BuildingId" });

            migrationBuilder.AddForeignKey(
                name: "FK_facultyBuildings_buildings_BuildingId",
                schema: "campus",
                table: "facultyBuildings",
                column: "BuildingId",
                principalSchema: "campus",
                principalTable: "buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_facultyBuildings_faculties_FacultyId",
                schema: "campus",
                table: "facultyBuildings",
                column: "FacultyId",
                principalSchema: "campus",
                principalTable: "faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_facultyBuildings_buildings_BuildingId",
                schema: "campus",
                table: "facultyBuildings");

            migrationBuilder.DropForeignKey(
                name: "FK_facultyBuildings_faculties_FacultyId",
                schema: "campus",
                table: "facultyBuildings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_facultyBuildings",
                schema: "campus",
                table: "facultyBuildings");

            migrationBuilder.RenameTable(
                name: "facultyBuildings",
                schema: "campus",
                newName: "FacultyBuilding",
                newSchema: "campus");

            migrationBuilder.RenameIndex(
                name: "IX_facultyBuildings_BuildingId",
                schema: "campus",
                table: "FacultyBuilding",
                newName: "IX_FacultyBuilding_BuildingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FacultyBuilding",
                schema: "campus",
                table: "FacultyBuilding",
                columns: new[] { "FacultyId", "BuildingId" });

            migrationBuilder.AddForeignKey(
                name: "FK_FacultyBuilding_buildings_BuildingId",
                schema: "campus",
                table: "FacultyBuilding",
                column: "BuildingId",
                principalSchema: "campus",
                principalTable: "buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FacultyBuilding_faculties_FacultyId",
                schema: "campus",
                table: "FacultyBuilding",
                column: "FacultyId",
                principalSchema: "campus",
                principalTable: "faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
