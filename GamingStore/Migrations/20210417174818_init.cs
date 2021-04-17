using Microsoft.EntityFrameworkCore.Migrations;

namespace GamingStore.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    ItemId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: false),
                    Price = table.Column<float>(nullable: false),
                    Brand = table.Column<string>(nullable: false),
                    StockCounter = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Category = table.Column<string>(nullable: false),
                    StarReview = table.Column<float>(nullable: false),
                    ImageUrl = table.Column<string>(nullable: false),
                    Active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.ItemId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Item");
        }
    }
}
