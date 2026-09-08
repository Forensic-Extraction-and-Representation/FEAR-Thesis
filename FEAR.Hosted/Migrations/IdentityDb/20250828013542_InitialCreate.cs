using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEAR.Hosted.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    PermissionSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionSetId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "SystemActions",
                columns: table => new
                {
                    SystemActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemActions", x => x.SystemActionId);
                });

            migrationBuilder.CreateTable(
                name: "UserRefreshTokens",
                columns: table => new
                {
                    UserRefreshTokenId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefreshToken = table.Column<string>(type: "TEXT", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRefreshTokens", x => x.UserRefreshTokenId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "PermissionSetRole",
                columns: table => new
                {
                    PermissionSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    RolesRoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionSetRole", x => new { x.PermissionSetId, x.RolesRoleId });
                    table.ForeignKey(
                        name: "FK_PermissionSetRole_Permissions_PermissionSetId",
                        column: x => x.PermissionSetId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionSetId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionSetRole_Roles_RolesRoleId",
                        column: x => x.RolesRoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionSetsApprovedActions",
                columns: table => new
                {
                    ApprovedActionsSystemActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedPermissionSetsPermissionSetId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionSetsApprovedActions", x => new { x.ApprovedActionsSystemActionId, x.ApprovedPermissionSetsPermissionSetId });
                    table.ForeignKey(
                        name: "FK_PermissionSetsApprovedActions_Permissions_ApprovedPermissionSetsPermissionSetId",
                        column: x => x.ApprovedPermissionSetsPermissionSetId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionSetId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionSetsApprovedActions_SystemActions_ApprovedActionsSystemActionId",
                        column: x => x.ApprovedActionsSystemActionId,
                        principalTable: "SystemActions",
                        principalColumn: "SystemActionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionSetsDeniedActions",
                columns: table => new
                {
                    DeniedActionsSystemActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeniedPermissionSetsPermissionSetId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionSetsDeniedActions", x => new { x.DeniedActionsSystemActionId, x.DeniedPermissionSetsPermissionSetId });
                    table.ForeignKey(
                        name: "FK_PermissionSetsDeniedActions_Permissions_DeniedPermissionSetsPermissionSetId",
                        column: x => x.DeniedPermissionSetsPermissionSetId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionSetId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionSetsDeniedActions_SystemActions_DeniedActionsSystemActionId",
                        column: x => x.DeniedActionsSystemActionId,
                        principalTable: "SystemActions",
                        principalColumn: "SystemActionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestigationRoleAssignments",
                columns: table => new
                {
                    InvestigationRoleAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvestigationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationRoleAssignments", x => x.InvestigationRoleAssignmentId);
                    table.ForeignKey(
                        name: "FK_InvestigationRoleAssignments_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestigationRoleAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemRoleAssignments",
                columns: table => new
                {
                    SystemRoleAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemRoleAssignments", x => x.SystemRoleAssignmentId);
                    table.ForeignKey(
                        name: "FK_SystemRoleAssignments_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SystemRoleAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSecrets",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    KeyName = table.Column<string>(type: "TEXT", nullable: false),
                    SecretData = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSecrets", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserSecrets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationRoleAssignments_RoleId",
                table: "InvestigationRoleAssignments",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationRoleAssignments_UserId",
                table: "InvestigationRoleAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionSetRole_RolesRoleId",
                table: "PermissionSetRole",
                column: "RolesRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionSetsApprovedActions_ApprovedPermissionSetsPermissionSetId",
                table: "PermissionSetsApprovedActions",
                column: "ApprovedPermissionSetsPermissionSetId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionSetsDeniedActions_DeniedPermissionSetsPermissionSetId",
                table: "PermissionSetsDeniedActions",
                column: "DeniedPermissionSetsPermissionSetId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemRoleAssignments_RoleId",
                table: "SystemRoleAssignments",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemRoleAssignments_UserId",
                table: "SystemRoleAssignments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestigationRoleAssignments");

            migrationBuilder.DropTable(
                name: "PermissionSetRole");

            migrationBuilder.DropTable(
                name: "PermissionSetsApprovedActions");

            migrationBuilder.DropTable(
                name: "PermissionSetsDeniedActions");

            migrationBuilder.DropTable(
                name: "SystemRoleAssignments");

            migrationBuilder.DropTable(
                name: "UserRefreshTokens");

            migrationBuilder.DropTable(
                name: "UserSecrets");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "SystemActions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
