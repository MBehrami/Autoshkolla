using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminApi.Migrations
{
    /// <inheritdoc />
    public partial class etestimi_manual_candidate_accounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CandidateAccounts_CandidateId",
                table: "CandidateAccounts");

            migrationBuilder.AlterColumn<int>(
                name: "CandidateId",
                table: "CandidateAccounts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "CandidateAccounts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "CandidateAccounts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 883, DateTimeKind.Local).AddTicks(274));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 883, DateTimeKind.Local).AddTicks(278));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 3,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 883, DateTimeKind.Local).AddTicks(279));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 4,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 883, DateTimeKind.Local).AddTicks(280));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 5,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 883, DateTimeKind.Local).AddTicks(282));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 6,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 883, DateTimeKind.Local).AddTicks(283));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 7,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 883, DateTimeKind.Local).AddTicks(284));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9407));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9411));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 3,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9412));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 4,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9414));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 5,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9416));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 6,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9417));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 7,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9419));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 8,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9421));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 9,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9423));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 10,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9424));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 13,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9426));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 14,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9427));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 15,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9429));

            migrationBuilder.UpdateData(
                table: "MenuGroup",
                keyColumn: "MenuGroupID",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(8192));

            migrationBuilder.UpdateData(
                table: "MenuGroup",
                keyColumn: "MenuGroupID",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(8237));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9636));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9638));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 3,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9639));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 4,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9641));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 5,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9642));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 6,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9644));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 7,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9645));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 8,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9646));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 9,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9648));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 10,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9649));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 11,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9650));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 12,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9652));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 13,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9653));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "SiteSettingsId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9862));

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "UserRoleId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(8534));

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "UserRoleId",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(8537));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9051));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 22, 51, 11, 882, DateTimeKind.Local).AddTicks(9057));

            migrationBuilder.CreateIndex(
                name: "IX_CandidateAccounts_CandidateId",
                table: "CandidateAccounts",
                column: "CandidateId",
                unique: true,
                filter: "[CandidateId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CandidateAccounts_CandidateId",
                table: "CandidateAccounts");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "CandidateAccounts");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "CandidateAccounts");

            migrationBuilder.AlterColumn<int>(
                name: "CandidateId",
                table: "CandidateAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2817));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2820));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 3,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2822));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 4,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2823));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 5,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2824));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 6,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2826));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 7,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2827));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2105));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2109));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 3,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2111));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 4,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2113));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 5,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2114));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 6,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2116));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 7,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2118));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 8,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2119));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 9,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2121));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 10,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2123));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 13,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2124));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 14,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2126));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 15,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2128));

            migrationBuilder.UpdateData(
                table: "MenuGroup",
                keyColumn: "MenuGroupID",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(1084));

            migrationBuilder.UpdateData(
                table: "MenuGroup",
                keyColumn: "MenuGroupID",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(1105));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2267));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2269));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 3,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2271));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 4,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2272));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 5,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2274));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 6,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2275));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 7,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2276));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 8,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2278));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 9,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2279));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 10,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2280));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 11,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2282));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 12,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2283));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 13,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2285));

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "SiteSettingsId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2465));

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "UserRoleId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(1338));

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "UserRoleId",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(1341));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(1773));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(1778));

            migrationBuilder.CreateIndex(
                name: "IX_CandidateAccounts_CandidateId",
                table: "CandidateAccounts",
                column: "CandidateId",
                unique: true);
        }
    }
}
