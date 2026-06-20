using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitterClone.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddRetweetsTable : Migration
{
    private static readonly string[] RetweeterTweetColumns = ["RetweeterId", "TweetId"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Retweets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RetweeterId = table.Column<Guid>(type: "uuid", nullable: false),
                TweetId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Retweets", x => x.Id);
                table.ForeignKey(
                    name: "FK_Retweets_tweets_TweetId",
                    column: x => x.TweetId,
                    principalTable: "tweets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Retweets_users_RetweeterId",
                    column: x => x.RetweeterId,
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Retweets_RetweeterId_TweetId",
            table: "Retweets",
            columns: RetweeterTweetColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Retweets_TweetId",
            table: "Retweets",
            column: "TweetId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Retweets");
    }
}
