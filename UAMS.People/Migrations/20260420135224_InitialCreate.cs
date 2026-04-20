using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UAMS.Campus.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "campus");

            migrationBuilder.CreateTable(
                name: "buildings",
                schema: "campus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_buildings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "department_managers",
                schema: "campus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_department_managers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "faculties",
                schema: "campus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faculties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "teachers",
                schema: "campus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CanDesign = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teachers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                schema: "campus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Handles = table.Column<int>(type: "integer", nullable: false),
                    DepartmentManagerId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_departments_department_managers_DepartmentManagerId",
                        column: x => x.DepartmentManagerId,
                        principalSchema: "campus",
                        principalTable: "department_managers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asset_managers",
                schema: "campus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FacultyId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_managers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_asset_managers_faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalSchema: "campus",
                        principalTable: "faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FacultyBuilding",
                schema: "campus",
                columns: table => new
                {
                    FacultyId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuildingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacultyBuilding", x => new { x.FacultyId, x.BuildingId });
                    table.ForeignKey(
                        name: "FK_FacultyBuilding_buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalSchema: "campus",
                        principalTable: "buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FacultyBuilding_faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalSchema: "campus",
                        principalTable: "faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "students",
                schema: "campus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FacultyId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_students_faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalSchema: "campus",
                        principalTable: "faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teacherFaculties",
                schema: "campus",
                columns: table => new
                {
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    FacultyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacherFaculties", x => new { x.TeacherId, x.FacultyId });
                    table.ForeignKey(
                        name: "FK_teacherFaculties_faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalSchema: "campus",
                        principalTable: "faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_teacherFaculties_teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalSchema: "campus",
                        principalTable: "teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainers",
                schema: "campus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainers_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "campus",
                        principalTable: "departments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_asset_managers_FacultyId",
                schema: "campus",
                table: "asset_managers",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_asset_managers_UserId",
                schema: "campus",
                table: "asset_managers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_department_managers_UserId",
                schema: "campus",
                table: "department_managers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_departments_DepartmentManagerId",
                schema: "campus",
                table: "departments",
                column: "DepartmentManagerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacultyBuilding_BuildingId",
                schema: "campus",
                table: "FacultyBuilding",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainers_DepartmentId",
                schema: "campus",
                table: "maintainers",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainers_UserId",
                schema: "campus",
                table: "maintainers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_students_FacultyId",
                schema: "campus",
                table: "students",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_students_UserId",
                schema: "campus",
                table: "students",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_teacherFaculties_FacultyId",
                schema: "campus",
                table: "teacherFaculties",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_teachers_UserId",
                schema: "campus",
                table: "teachers",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asset_managers",
                schema: "campus");

            migrationBuilder.DropTable(
                name: "FacultyBuilding",
                schema: "campus");

            migrationBuilder.DropTable(
                name: "maintainers",
                schema: "campus");

            migrationBuilder.DropTable(
                name: "students",
                schema: "campus");

            migrationBuilder.DropTable(
                name: "teacherFaculties",
                schema: "campus");

            migrationBuilder.DropTable(
                name: "buildings",
                schema: "campus");

            migrationBuilder.DropTable(
                name: "departments",
                schema: "campus");

            migrationBuilder.DropTable(
                name: "faculties",
                schema: "campus");

            migrationBuilder.DropTable(
                name: "teachers",
                schema: "campus");

            migrationBuilder.DropTable(
                name: "department_managers",
                schema: "campus");
        }
    }
}
