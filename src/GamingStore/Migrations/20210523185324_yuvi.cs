using Microsoft.EntityFrameworkCore.Migrations;

namespace GamingStore.Migrations
{
    public partial class yuvi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Item");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Item",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
