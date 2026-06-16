using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitterClone.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Bio = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                AvatarUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "follows",
            columns: table => new
            {
                FollowerId = table.Column<Guid>(type: "uuid", nullable: false),
                FolloweeId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_follows", x => new { x.FollowerId, x.FolloweeId });
                table.ForeignKey(
                    name: "FK_follows_users_FolloweeId",
                    column: x => x.FolloweeId,
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_follows_users_FollowerId",
                    column: x => x.FollowerId,
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "refresh_tokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_refresh_tokens_users_UserId",
                    column: x => x.UserId,
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "tweets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                Text = table.Column<string>(type: "character varying(280)", maxLength: 280, nullable: false),
                ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                ImageUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_tweets", x => x.Id);
                table.ForeignKey(
                    name: "FK_tweets_tweets_ParentId",
                    column: x => x.ParentId,
                    principalTable: "tweets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_tweets_users_AuthorId",
                    column: x => x.AuthorId,
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "likes",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                TweetId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_likes", x => new { x.UserId, x.TweetId });
                table.ForeignKey(
                    name: "FK_likes_tweets_TweetId",
                    column: x => x.TweetId,
                    principalTable: "tweets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_likes_users_UserId",
                    column: x => x.UserId,
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_follows_FolloweeId",
            table: "follows",
            column: "FolloweeId");

        migrationBuilder.CreateIndex(
            name: "IX_likes_TweetId",
            table: "likes",
            column: "TweetId");

        migrationBuilder.CreateIndex(
            name: "IX_refresh_tokens_TokenHash",
            table: "refresh_tokens",
            column: "TokenHash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_refresh_tokens_UserId",
            table: "refresh_tokens",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_tweets_AuthorId",
            table: "tweets",
            column: "AuthorId");

        migrationBuilder.CreateIndex(
            name: "IX_tweets_CreatedAt",
            table: "tweets",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_tweets_ParentId",
            table: "tweets",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_users_Email",
            table: "users",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_users_Username",
            table: "users",
            column: "Username",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "follows");
        migrationBuilder.DropTable(name: "likes");
        migrationBuilder.DropTable(name: "refresh_tokens");
        migrationBuilder.DropTable(name: "tweets");
        migrationBuilder.DropTable(name: "users");
    }
}
