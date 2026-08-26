using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateAdminHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("e01837ee-e5c5-48db-98c2-cdc266139856"),
                column: "HashPassword",
                value: "AQAAAAIAAYagAAAAEIlcXEQY7EF9J8g4o8G+1C8GSqLEQ4pZCfp5j5ygZ3kJc73c3EiCdl4C5baLWEp00Q==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("e01837ee-e5c5-48db-98c2-cdc266139856"),
                column: "HashPassword",
                value: "");
        }
    }
}
