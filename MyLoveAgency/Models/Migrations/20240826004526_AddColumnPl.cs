using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLoveAgency.Models.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnPl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                 name: "name_pl",
                 table: "TypeService",
                 type: "nvarchar(50)",
                 nullable: true);

            migrationBuilder.AddColumn<string>(
                 name: "description_pl",
                 table: "TypeService",
                 type: "nvarchar(max)",
                 nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
