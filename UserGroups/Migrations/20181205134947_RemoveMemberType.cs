using Microsoft.EntityFrameworkCore.Migrations;

namespace UserGroups.Migrations
{
    public partial class RemoveMemberType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "GroupMember");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "GroupMember",
                nullable: false,
                defaultValue: 0);
        }
    }
}
