using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BlazingPizza.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(nullable: true),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    DeliveryLocation_Latitude = table.Column<double>(nullable: true),
                    DeliveryLocation_Longitude = table.Column<double>(nullable: true),
                    CurrentLocation_Latitude = table.Column<double>(nullable: true),
                    CurrentLocation_Longitude = table.Column<double>(nullable: true),
                    Status = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                });

            migrationBuilder.CreateTable(
                name: "Specials",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    BasePrice = table.Column<decimal>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    ImageUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Toppings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Price = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Toppings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pizzas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<int>(nullable: false),
                    SpecialId = table.Column<int>(nullable: false),
                    Size = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pizzas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pizzas_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pizzas_Specials_SpecialId",
                        column: x => x.SpecialId,
                        principalTable: "Specials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PizzaTopping",
                columns: table => new
                {
                    ToppingId = table.Column<int>(nullable: false),
                    PizzaId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PizzaTopping", x => new { x.PizzaId, x.ToppingId });
                    table.ForeignKey(
                        name: "FK_PizzaTopping_Pizzas_PizzaId",
                        column: x => x.PizzaId,
                        principalTable: "Pizzas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PizzaTopping_Toppings_ToppingId",
                        column: x => x.ToppingId,
                        principalTable: "Toppings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Specials",
                columns: new[] { "Id", "BasePrice", "Description", "ImageUrl", "Name" },
                values: new object[,]
                {
                    { 1, 9.99m, "It's cheesy and delicious. Why wouldn't you want one?", "img/pizzas/cheese.jpg", "Basic Cheese Pizza" },
                    { 2, 11.99m, "It has EVERY kind of bacon", "img/pizzas/bacon.jpg", "The Baconatorizor" },
                    { 3, 10.50m, "It's the pizza you grew up with, but Blazing hot!", "img/pizzas/pepperoni.jpg", "Classic pepperoni" },
                    { 4, 12.75m, "Spicy chicken, hot sauce and bleu cheese, guaranteed to warm you up", "img/pizzas/meaty.jpg", "Buffalo chicken" },
                    { 5, 11.00m, "It has mushrooms. Isn't that obvious?", "img/pizzas/mushroom.jpg", "Mushroom Lovers" },
                    { 6, 10.25m, "When in London...", "img/pizzas/brit.jpg", "The Brit" },
                    { 7, 11.50m, "It's like salad, but on a pizza", "img/pizzas/salad.jpg", "Veggie Delight" },
                    { 8, 9.99m, "Traditional Italian pizza with tomatoes and basil", "img/pizzas/margherita.jpg", "Margherita" }
                });

            migrationBuilder.InsertData(
                table: "Toppings",
                columns: new[] { "Id", "Name", "Price" },
                values: new object[,]
                {
                    { 13, "Served on a silver platter", 250.99m },
                    { 14, "Lobster on top", 64.50m },
                    { 15, "Sturgeon caviar", 101.75m },
                    { 16, "Artichoke hearts", 3.40m },
                    { 20, "Blazing hot peppers", 4.20m },
                    { 18, "Basil", 1.50m },
                    { 19, "Steak (medium-rare)", 8.50m },
                    { 12, "Venison meatballs", 2.50m },
                    { 17, "Fresh tomatos", 1.50m },
                    { 11, "Duck sausage", 3.20m },
                    { 7, "Bell peppers", 1.00m },
                    { 9, "Mushrooms", 1.00m },
                    { 8, "Onions", 1.00m },
                    { 21, "Buffalo chicken", 5.00m },
                    { 6, "Fresh-baked scones", 4.50m },
                    { 5, "Tea and crumpets", 5.00m },
                    { 4, "Canadian bacon", 2.99m },
                    { 3, "British bacon", 2.99m },
                    { 2, "American bacon", 2.99m },
                    { 1, "Extra cheese", 2.50m },
                    { 10, "Pepperoni", 1.00m },
                    { 22, "Blue cheese", 2.50m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pizzas_OrderId",
                table: "Pizzas",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Pizzas_SpecialId",
                table: "Pizzas",
                column: "SpecialId");

            migrationBuilder.CreateIndex(
                name: "IX_PizzaTopping_ToppingId",
                table: "PizzaTopping",
                column: "ToppingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PizzaTopping");

            migrationBuilder.DropTable(
                name: "Pizzas");

            migrationBuilder.DropTable(
                name: "Toppings");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Specials");
        }
    }
}
