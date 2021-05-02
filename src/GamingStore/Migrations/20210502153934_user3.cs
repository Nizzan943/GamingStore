using Microsoft.EntityFrameworkCore.Migrations;

namespace GamingStore.Migrations
{
    public partial class user3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryNotes",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Address");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Address",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Address",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address1",
                table: "Address",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Address",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address1",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Address");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Address",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Address",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryNotes",
                table: "Address",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Address",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Address",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Address",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
