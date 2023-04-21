using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations
{
    /// <inheritdoc />
    public partial class NotInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Maintainers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiscordUserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    CachedName = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintainers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManagedModlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MachineURL = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagedModlists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscribedChannels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiscordChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DiscordGuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    CachedName = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscribedChannels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaintainerManagedModlist",
                columns: table => new
                {
                    MaintainersId = table.Column<int>(type: "INTEGER", nullable: false),
                    ManagedModlistsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintainerManagedModlist", x => new { x.MaintainersId, x.ManagedModlistsId });
                    table.ForeignKey(
                        name: "FK_MaintainerManagedModlist_Maintainers_MaintainersId",
                        column: x => x.MaintainersId,
                        principalTable: "Maintainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintainerManagedModlist_ManagedModlists_ManagedModlistsId",
                        column: x => x.ManagedModlistsId,
                        principalTable: "ManagedModlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PingRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiscordRoleId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DiscordGuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ManagedModlistId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PingRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PingRoles_ManagedModlists_ManagedModlistId",
                        column: x => x.ManagedModlistId,
                        principalTable: "ManagedModlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Releases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ManagedModlistId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Releases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Releases_ManagedModlists_ManagedModlistId",
                        column: x => x.ManagedModlistId,
                        principalTable: "ManagedModlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ManagedModlistId = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseTemplates_ManagedModlists_ManagedModlistId",
                        column: x => x.ManagedModlistId,
                        principalTable: "ManagedModlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManagedModlistSubscribedChannel",
                columns: table => new
                {
                    ManagedModlistsId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubscribedChannelsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagedModlistSubscribedChannel", x => new { x.ManagedModlistsId, x.SubscribedChannelsId });
                    table.ForeignKey(
                        name: "FK_ManagedModlistSubscribedChannel_ManagedModlists_ManagedModlistsId",
                        column: x => x.ManagedModlistsId,
                        principalTable: "ManagedModlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManagedModlistSubscribedChannel_SubscribedChannels_SubscribedChannelsId",
                        column: x => x.SubscribedChannelsId,
                        principalTable: "SubscribedChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    DiscordMessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ManagedModlistId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubscribedChannelId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaintainerId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReleaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseMessages_Maintainers_MaintainerId",
                        column: x => x.MaintainerId,
                        principalTable: "Maintainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseMessages_ManagedModlists_ManagedModlistId",
                        column: x => x.ManagedModlistId,
                        principalTable: "ManagedModlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseMessages_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseMessages_SubscribedChannels_SubscribedChannelId",
                        column: x => x.SubscribedChannelId,
                        principalTable: "SubscribedChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintainerManagedModlist_ManagedModlistsId",
                table: "MaintainerManagedModlist",
                column: "ManagedModlistsId");

            migrationBuilder.CreateIndex(
                name: "IX_Maintainers_DiscordUserId",
                table: "Maintainers",
                column: "DiscordUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManagedModlists_MachineURL",
                table: "ManagedModlists",
                column: "MachineURL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManagedModlistSubscribedChannel_SubscribedChannelsId",
                table: "ManagedModlistSubscribedChannel",
                column: "SubscribedChannelsId");

            migrationBuilder.CreateIndex(
                name: "IX_PingRoles_DiscordRoleId",
                table: "PingRoles",
                column: "DiscordRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PingRoles_ManagedModlistId",
                table: "PingRoles",
                column: "ManagedModlistId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseMessages_DiscordMessageId",
                table: "ReleaseMessages",
                column: "DiscordMessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseMessages_MaintainerId",
                table: "ReleaseMessages",
                column: "MaintainerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseMessages_ManagedModlistId",
                table: "ReleaseMessages",
                column: "ManagedModlistId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseMessages_ReleaseId",
                table: "ReleaseMessages",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseMessages_SubscribedChannelId",
                table: "ReleaseMessages",
                column: "SubscribedChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_ManagedModlistId",
                table: "Releases",
                column: "ManagedModlistId");

            */
            migrationBuilder.CreateIndex(
                name: "IX_ReleaseTemplates_ManagedModlistId",
                table: "ReleaseTemplates",
                column: "ManagedModlistId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscribedChannels_DiscordChannelId",
                table: "SubscribedChannels",
                column: "DiscordChannelId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintainerManagedModlist");

            migrationBuilder.DropTable(
                name: "ManagedModlistSubscribedChannel");

            migrationBuilder.DropTable(
                name: "PingRoles");

            migrationBuilder.DropTable(
                name: "ReleaseMessages");

            migrationBuilder.DropTable(
                name: "ReleaseTemplates");

            migrationBuilder.DropTable(
                name: "Maintainers");

            migrationBuilder.DropTable(
                name: "Releases");

            migrationBuilder.DropTable(
                name: "SubscribedChannels");

            migrationBuilder.DropTable(
                name: "ManagedModlists");
        }
    }
}
