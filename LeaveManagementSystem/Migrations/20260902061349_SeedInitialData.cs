using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LeaveManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveBalances_Employees_EmployeeId",
                table: "LeaveBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveBalances_LeaveTypes_LeaveTypeId",
                table: "LeaveBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_Employees_EmployeeId",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_LeaveTypes_LeaveTypeId",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveBalances_EmployeeId_LeaveTypeId",
                table: "LeaveBalances");

            migrationBuilder.DropIndex(
                name: "IX_Employees_Email",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeCode",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeId", "Department", "Email", "EmployeeCode", "IsActive", "JoiningDate", "Name" },
                values: new object[,]
                {
                    { 1, "IT", "priya@example.com", "EMP001", true, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Priya" },
                    { 2, "HR", "poojitha@example.com", "EMP002", true, new DateTime(2024, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Poojitha" },
                    { 3, "Finance", "arjun@example.com", "EMP003", true, new DateTime(2023, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Arjun" },
                    { 4, "Administration", "saritha@example.com", "EMP004", true, new DateTime(2022, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Saritha" }
                });

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeId",
                keyValue: 1,
                columns: new[] { "LeaveTypeName", "MaximumDays" },
                values: new object[] { "Annual Leave", 15 });

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeId",
                keyValue: 3,
                columns: new[] { "LeaveTypeName", "MaximumDays" },
                values: new object[] { "Casual Leave", 5 });

            migrationBuilder.InsertData(
                table: "LeaveBalances",
                columns: new[] { "LeaveBalanceId", "EmployeeId", "LeaveTypeId", "TotalDays", "UsedDays" },
                values: new object[,]
                {
                    { 1, 1, 1, 15, 0 },
                    { 2, 1, 2, 10, 0 },
                    { 3, 1, 3, 5, 0 },
                    { 4, 2, 1, 15, 0 },
                    { 5, 2, 2, 10, 0 },
                    { 6, 2, 3, 5, 0 },
                    { 7, 3, 1, 15, 0 },
                    { 8, 3, 2, 10, 0 },
                    { 9, 3, 3, 5, 0 },
                    { 10, 4, 1, 15, 0 },
                    { 11, 4, 2, 10, 0 },
                    { 12, 4, 3, 5, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_EmployeeId",
                table: "LeaveBalances",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveBalances_Employees_EmployeeId",
                table: "LeaveBalances",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveBalances_LeaveTypes_LeaveTypeId",
                table: "LeaveBalances",
                column: "LeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "LeaveTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_Employees_EmployeeId",
                table: "LeaveRequests",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_LeaveTypes_LeaveTypeId",
                table: "LeaveRequests",
                column: "LeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "LeaveTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveBalances_Employees_EmployeeId",
                table: "LeaveBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveBalances_LeaveTypes_LeaveTypeId",
                table: "LeaveBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_Employees_EmployeeId",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_LeaveTypes_LeaveTypeId",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveBalances_EmployeeId",
                table: "LeaveBalances");

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4);

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeCode",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeId",
                keyValue: 1,
                columns: new[] { "LeaveTypeName", "MaximumDays" },
                values: new object[] { "Casual Leave", 12 });

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeId",
                keyValue: 3,
                columns: new[] { "LeaveTypeName", "MaximumDays" },
                values: new object[] { "Earned Leave", 15 });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_EmployeeId_LeaveTypeId",
                table: "LeaveBalances",
                columns: new[] { "EmployeeId", "LeaveTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveBalances_Employees_EmployeeId",
                table: "LeaveBalances",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveBalances_LeaveTypes_LeaveTypeId",
                table: "LeaveBalances",
                column: "LeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "LeaveTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_Employees_EmployeeId",
                table: "LeaveRequests",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_LeaveTypes_LeaveTypeId",
                table: "LeaveRequests",
                column: "LeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "LeaveTypeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
