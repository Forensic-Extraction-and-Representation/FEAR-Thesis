using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEAR.Hosted.Migrations.HostedSystemDb
{
    /// <inheritdoc />
    public partial class CreateHostedSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemWideProperties",
                columns: table => new
                {
                    SystemWidePropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemWideProperties", x => x.SystemWidePropertyId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemWideProperties");
        }
    }
}
