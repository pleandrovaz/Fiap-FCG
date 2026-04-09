using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCG.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarIdJogoEmPromocao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IdJogo",
                table: "promocoes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_promocoes_IdJogo",
                table: "promocoes",
                column: "IdJogo");

            migrationBuilder.AddForeignKey(
                name: "FK_promocoes_jogos_IdJogo",
                table: "promocoes",
                column: "IdJogo",
                principalTable: "jogos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_promocoes_jogos_IdJogo",
                table: "promocoes");

            migrationBuilder.DropIndex(
                name: "IX_promocoes_IdJogo",
                table: "promocoes");

            migrationBuilder.DropColumn(
                name: "IdJogo",
                table: "promocoes");
        }
    }
}
