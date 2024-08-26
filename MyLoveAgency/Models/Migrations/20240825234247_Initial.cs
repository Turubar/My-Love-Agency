using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLoveAgency.Models.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "pl",
                table: "Localization",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
