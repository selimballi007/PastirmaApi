using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PastirmaApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRepliedToContactFormSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_replied",
                table: "contact_form_submissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "replied_at",
                table: "contact_form_submissions",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_replied",
                table: "contact_form_submissions");

            migrationBuilder.DropColumn(
                name: "replied_at",
                table: "contact_form_submissions");
        }
    }
}
