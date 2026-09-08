using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEAR.Hosted.Migrations.InvestigationManagementDb
{
    /// <inheritdoc />
    public partial class CreateColorMapSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ColorMapSets",
                columns: table => new
                {
                    ColorMapSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    EntityDiscriminator = table.Column<int>(type: "INTEGER", nullable: false),
                    EntityId = table.Column<string>(type: "TEXT", nullable: true),
                    ColorMaps = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorMapSets", x => x.ColorMapSetId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColorMapSets");
        }
    }
}
