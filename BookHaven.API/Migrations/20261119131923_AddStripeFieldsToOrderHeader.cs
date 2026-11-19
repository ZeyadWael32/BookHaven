using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookHaven.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeFieldsToOrderHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "OrderHeaders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSessionId",
                table: "OrderHeaders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_PaymentIntentId",
                table: "OrderHeaders",
                column: "PaymentIntentId",
                unique: true,
                filter: "[PaymentIntentId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderHeaders_PaymentIntentId",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "StripeSessionId",
                table: "OrderHeaders");
        }
    }
}
