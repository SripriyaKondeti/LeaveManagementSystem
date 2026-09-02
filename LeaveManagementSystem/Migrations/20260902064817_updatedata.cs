using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class updatedata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "JoiningDate",
                value: new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "JoiningDate",
                value: new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "JoiningDate",
                value: new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "JoiningDate",
                value: new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 3,
                column: "TotalDays",
                value: 12);

            migrationBuilder.UpdateData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 6,
                column: "TotalDays",
                value: 12);

            migrationBuilder.UpdateData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 9,
                column: "TotalDays",
                value: 12);

            migrationBuilder.UpdateData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 12,
                column: "TotalDays",
                value: 12);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "JoiningDate",
                value: new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "JoiningDate",
                value: new DateTime(2024, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "JoiningDate",
                value: new DateTime(2023, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "JoiningDate",
                value: new DateTime(2022, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 3,
                column: "TotalDays",
                value: 5);

            migrationBuilder.UpdateData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 6,
                column: "TotalDays",
                value: 5);

            migrationBuilder.UpdateData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 9,
                column: "TotalDays",
                value: 5);

            migrationBuilder.UpdateData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: 12,
                column: "TotalDays",
                value: 5);
        }
    }
}
