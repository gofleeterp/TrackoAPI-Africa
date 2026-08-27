using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IWLT.TrackoAPI.Subscription.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TenantKey = table.Column<string>(nullable: true),
                    ApplicationKey = table.Column<string>(nullable: true),
                    UserKey = table.Column<long>(nullable: false),
                    RequestTimestamp = table.Column<DateTime>(nullable: false),
                    ResponseTimestamp = table.Column<DateTime>(nullable: false),
                    RequestContent = table.Column<string>(nullable: true),
                    ResponseContent = table.Column<string>(nullable: true),
                    RequestHeaders = table.Column<string>(nullable: true),
                    ResponseHeaders = table.Column<string>(nullable: true),
                    Uri = table.Column<string>(nullable: true),
                    RequestMethod = table.Column<string>(nullable: true),
                    IP = table.Column<string>(nullable: true),
                    ResponseStatusCode = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ApplicationName = table.Column<string>(maxLength: 50, nullable: true),
                    ApplicationType = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    UpdateUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    ClientKey = table.Column<string>(maxLength: 300, nullable: false),
                    ShortName = table.Column<string>(maxLength: 100, nullable: true),
                    Secret = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    ConnectionString = table.Column<string>(nullable: false),
                    PostalAddress = table.Column<string>(maxLength: 200, nullable: false),
                    PANNo = table.Column<string>(maxLength: 10, nullable: true),
                    PhoneNumber = table.Column<string>(nullable: false),
                    EmailAddress = table.Column<string>(nullable: false),
                    WebAddress = table.Column<string>(maxLength: 200, nullable: true),
                    LogType = table.Column<int>(nullable: false),
                    ServerUrl = table.Column<string>(nullable: true),
                    RemoteBackupPath = table.Column<string>(maxLength: 500, nullable: true),
                    IsSingleUserMode = table.Column<bool>(nullable: false),
                    AccessCode = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BackupLogs",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    TenantId = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    FinishDate = table.Column<DateTime>(nullable: false),
                    LocalFilePath = table.Column<string>(maxLength: 500, nullable: true),
                    LocalFileSize = table.Column<double>(nullable: false),
                    IsPublished = table.Column<bool>(nullable: false),
                    RemoteServerPath = table.Column<string>(maxLength: 4000, nullable: true),
                    RemoteFileSize = table.Column<double>(nullable: false),
                    IsBackupFailed = table.Column<bool>(nullable: false),
                    Exception = table.Column<string>(maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BackupLogs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantApplications",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ApplicationId = table.Column<string>(nullable: false),
                    TenantId = table.Column<string>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    UpdateUrl = table.Column<string>(nullable: true),
                    NoOfActiveUsers = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantApplications_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantApplications_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Integrations",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: true),
                    OriginHost = table.Column<string>(nullable: true),
                    Token = table.Column<string>(maxLength: 200, nullable: true),
                    EventTypeEventCode = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Integrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    EventCode = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: true),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    SubscriberId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.EventCode);
                    table.ForeignKey(
                        name: "FK_EventTypes_Integrations_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Integrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    JobLogId = table.Column<string>(maxLength: 255, nullable: true),
                    EventLogId = table.Column<string>(nullable: true),
                    EventCode = table.Column<int>(nullable: true),
                    SenderId = table.Column<string>(nullable: true),
                    TenantId = table.Column<string>(nullable: true),
                    EventBody = table.Column<string>(nullable: true),
                    IsProcessed = table.Column<bool>(nullable: false),
                    Error = table.Column<string>(nullable: true),
                    ProcessedTime = table.Column<DateTimeOffset>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_EventTypes_EventCode",
                        column: x => x.EventCode,
                        principalTable: "EventTypes",
                        principalColumn: "EventCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobs_Integrations_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Integrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApplicationName",
                table: "Applications",
                column: "ApplicationName",
                unique: true,
                filter: "[ApplicationName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BackupLogs_TenantId",
                table: "BackupLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_Name",
                table: "EventTypes",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_SubscriberId",
                table: "EventTypes",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_EventTypeEventCode",
                table: "Integrations",
                column: "EventTypeEventCode");

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_Name",
                table: "Integrations",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_EventCode",
                table: "Jobs",
                column: "EventCode");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_SenderId",
                table: "Jobs",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_TenantId",
                table: "Jobs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantApplications_ApplicationId",
                table: "TenantApplications",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantApplications_TenantId",
                table: "TenantApplications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_AccessCode",
                table: "Tenants",
                column: "AccessCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ClientKey",
                table: "Tenants",
                column: "ClientKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                table: "Tenants",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_PANNo",
                table: "Tenants",
                column: "PANNo",
                unique: true,
                filter: "[PANNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ShortName",
                table: "Tenants",
                column: "ShortName",
                unique: true,
                filter: "[ShortName] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Integrations_EventTypes_EventTypeEventCode",
                table: "Integrations",
                column: "EventTypeEventCode",
                principalTable: "EventTypes",
                principalColumn: "EventCode",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventTypes_Integrations_SubscriberId",
                table: "EventTypes");

            migrationBuilder.DropTable(
                name: "ApiLog");

            migrationBuilder.DropTable(
                name: "BackupLogs");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "TenantApplications");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Integrations");

            migrationBuilder.DropTable(
                name: "EventTypes");
        }
    }
}
