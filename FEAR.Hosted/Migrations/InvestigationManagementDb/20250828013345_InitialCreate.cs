using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEAR.Hosted.Migrations.InvestigationManagementDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvestigationConfigurations",
                columns: table => new
                {
                    InvestigationConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    HostedUri = table.Column<string>(type: "TEXT", nullable: false),
                    InvestigationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Configuration = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationConfigurations", x => x.InvestigationConfigurationId);
                });

            migrationBuilder.CreateTable(
                name: "InvestigationQueries",
                columns: table => new
                {
                    InvestigationQueryId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvestigationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    QueryText = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp", nullable: false),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationQueries", x => x.InvestigationQueryId);
                });

            migrationBuilder.CreateTable(
                name: "Investigations",
                columns: table => new
                {
                    InvestigationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp", nullable: false),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CaseNumber = table.Column<string>(type: "TEXT", nullable: false),
                    CaseType = table.Column<string>(type: "TEXT", nullable: false),
                    CaseStatus = table.Column<string>(type: "TEXT", nullable: false),
                    CaseDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    Namespace = table.Column<string>(type: "TEXT", nullable: false),
                    NamespaceAbbrev = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Investigations", x => x.InvestigationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestigationConfigurations");

            migrationBuilder.DropTable(
                name: "InvestigationQueries");

            migrationBuilder.DropTable(
                name: "Investigations");
        }
    }
}
