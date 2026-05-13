using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UAMS.Campus.Migrations
{
    /// <inheritdoc />
    public partial class department_updated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_departments_department_managers_DepartmentManagerId",
                schema: "campus",
                table: "departments");

            migrationBuilder.AlterColumn<Guid>(
                name: "DepartmentManagerId",
                schema: "campus",
                table: "departments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_departments_department_managers_DepartmentManagerId",
                schema: "campus",
                table: "departments",
                column: "DepartmentManagerId",
                principalSchema: "campus",
                principalTable: "department_managers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_departments_department_managers_DepartmentManagerId",
                schema: "campus",
                table: "departments");

            migrationBuilder.AlterColumn<Guid>(
                name: "DepartmentManagerId",
                schema: "campus",
                table: "departments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_departments_department_managers_DepartmentManagerId",
                schema: "campus",
                table: "departments",
                column: "DepartmentManagerId",
                principalSchema: "campus",
                principalTable: "department_managers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
