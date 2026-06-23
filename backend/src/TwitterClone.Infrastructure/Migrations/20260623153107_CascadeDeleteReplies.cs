using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitterClone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tweets_tweets_ParentId",
                table: "tweets");

            migrationBuilder.AddForeignKey(
                name: "FK_tweets_tweets_ParentId",
                table: "tweets",
                column: "ParentId",
                principalTable: "tweets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tweets_tweets_ParentId",
                table: "tweets");

            migrationBuilder.AddForeignKey(
                name: "FK_tweets_tweets_ParentId",
                table: "tweets",
                column: "ParentId",
                principalTable: "tweets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
