using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class vendorRatingRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Vendors_VendorId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_VendorId",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "Ratings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VendorId",
                table: "Ratings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_VendorId",
                table: "Ratings",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Vendors_VendorId",
                table: "Ratings",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
