using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Infrastructure.Persistence;

public class DatabaseSeeder(AppDbContext db, IPasswordHasher passwordHasher)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(ct))
        {
            return;
        }

        var hash = passwordHasher.Hash("Seed1234!");

        var alice  = User.Create("alice",  "alice@example.com",  hash);
        var bob    = User.Create("bob",    "bob@example.com",    hash);
        var carol  = User.Create("carol",  "carol@example.com",  hash);
        var dave   = User.Create("dave",   "dave@example.com",   hash);
        var eve    = User.Create("eve",    "eve@example.com",    hash);
        var frank  = User.Create("frank",  "frank@example.com",  hash);
        var grace  = User.Create("grace",  "grace@example.com",  hash);
        var henry  = User.Create("henry",  "henry@example.com",  hash);
        var iris   = User.Create("iris",   "iris@example.com",   hash);
        var jack   = User.Create("jack",   "jack@example.com",   hash);

        db.Users.AddRange(alice, bob, carol, dave, eve, frank, grace, henry, iris, jack);

        // Tweets per user (3-5 each)
        var aliceTweets = new[]
        {
            Tweet.Create(alice.Id, "Just shipped a new feature using .NET 10 minimal APIs. The DX keeps getting better!"),
            Tweet.Create(alice.Id, "Hot take: Clean Architecture pays off after the first refactor, not before."),
            Tweet.Create(alice.Id, "Morning coffee + a green CI pipeline = perfect start to the day."),
            Tweet.Create(alice.Id, "Anyone else using Testcontainers for integration tests? Game changer."),
        };

        var bobTweets = new[]
        {
            Tweet.Create(bob.Id, "Reminder: nullable reference types are your friend, not your enemy."),
            Tweet.Create(bob.Id, "Spent 2 hours debugging a timezone issue. UTC everywhere, always."),
            Tweet.Create(bob.Id, "EF Core 10 performance improvements are real. Benchmarks don't lie."),
        };

        var carolTweets = new[]
        {
            Tweet.Create(carol.Id, "Vue 3 + Pinia is such a satisfying combo. State management finally makes sense."),
            Tweet.Create(carol.Id, "Tailwind JIT mode makes prototyping so fast it almost feels like cheating."),
            Tweet.Create(carol.Id, "TypeScript strict mode: painful for a day, saves you for a lifetime."),
            Tweet.Create(carol.Id, "Just discovered Playwright. Writing E2E tests has never been this painless."),
        };

        var daveTweets = new[]
        {
            Tweet.Create(dave.Id, "Docker compose makes onboarding new devs so much smoother."),
            Tweet.Create(dave.Id, "PostgreSQL JSONB columns are underrated for semi-structured data."),
            Tweet.Create(dave.Id, "Wrote my first custom EF Core interceptor today. Surprisingly elegant."),
        };

        var eveTweets = new[]
        {
            Tweet.Create(eve.Id, "Commit early, commit often. Future you will thank present you."),
            Tweet.Create(eve.Id, "Conventional Commits + semantic-release = automated changelogs. Zero effort."),
            Tweet.Create(eve.Id, "Code review tip: praise what is done well, not just what needs fixing."),
            Tweet.Create(eve.Id, "Hot take: the README is part of the product."),
        };

        var frankTweets = new[]
        {
            Tweet.Create(frank.Id, "SignalR real-time updates feel magical compared to polling."),
            Tweet.Create(frank.Id, "BCrypt is slow by design. That's the point."),
            Tweet.Create(frank.Id, "JWT access token + httpOnly refresh cookie is the auth setup I keep coming back to."),
        };

        var graceTweets = new[]
        {
            Tweet.Create(grace.Id, "NetArchTest enforcing layer boundaries automatically is underappreciated."),
            Tweet.Create(grace.Id, "FluentAssertions makes test failures actually readable."),
            Tweet.Create(grace.Id, "Coverage metrics are a floor, not a ceiling."),
            Tweet.Create(grace.Id, "Domain model with no external dependencies is a joy to test."),
        };

        var henryTweets = new[]
        {
            Tweet.Create(henry.Id, "Vite dev server starts in under a second. Webpack who?"),
            Tweet.Create(henry.Id, "Composables over mixins. Always. No debate."),
            Tweet.Create(henry.Id, "Axios interceptors for token refresh: set it up once, forget about it."),
        };

        var irisTweets = new[]
        {
            Tweet.Create(iris.Id, "Mobile-first CSS is not just a buzzword. Start small and expand."),
            Tweet.Create(iris.Id, "Accessibility is not a feature request. It is a requirement."),
            Tweet.Create(iris.Id, "Dark mode support in under 10 lines of Tailwind. I love CSS variables."),
            Tweet.Create(iris.Id, "Component isolation + Vitest = fast, reliable UI tests."),
        };

        var jackTweets = new[]
        {
            Tweet.Create(jack.Id, "When in doubt, ship it and iterate. Perfect is the enemy of done."),
            Tweet.Create(jack.Id, "Environment variables in .env.example: document your secrets without exposing them."),
            Tweet.Create(jack.Id, "The best architecture is the one your team can actually maintain."),
        };

        db.Tweets.AddRange(aliceTweets);
        db.Tweets.AddRange(bobTweets);
        db.Tweets.AddRange(carolTweets);
        db.Tweets.AddRange(daveTweets);
        db.Tweets.AddRange(eveTweets);
        db.Tweets.AddRange(frankTweets);
        db.Tweets.AddRange(graceTweets);
        db.Tweets.AddRange(henryTweets);
        db.Tweets.AddRange(irisTweets);
        db.Tweets.AddRange(jackTweets);

        // Follow relationships
        db.Follows.AddRange(
            Follow.Create(alice.Id,  bob.Id),
            Follow.Create(alice.Id,  carol.Id),
            Follow.Create(bob.Id,    alice.Id),
            Follow.Create(bob.Id,    dave.Id),
            Follow.Create(carol.Id,  alice.Id),
            Follow.Create(carol.Id,  eve.Id),
            Follow.Create(dave.Id,   bob.Id),
            Follow.Create(dave.Id,   frank.Id),
            Follow.Create(eve.Id,    carol.Id),
            Follow.Create(eve.Id,    frank.Id),
            Follow.Create(frank.Id,  grace.Id),
            Follow.Create(frank.Id,  eve.Id),
            Follow.Create(grace.Id,  henry.Id),
            Follow.Create(grace.Id,  alice.Id),
            Follow.Create(henry.Id,  iris.Id),
            Follow.Create(henry.Id,  grace.Id),
            Follow.Create(iris.Id,   jack.Id),
            Follow.Create(iris.Id,   henry.Id),
            Follow.Create(jack.Id,   alice.Id),
            Follow.Create(jack.Id,   iris.Id)
        );

        // Likes: each user likes 2-4 tweets from people they follow
        db.Likes.AddRange(
            // alice follows bob and carol
            Like.Create(alice.Id,  bobTweets[0].Id),
            Like.Create(alice.Id,  bobTweets[1].Id),
            Like.Create(alice.Id,  carolTweets[0].Id),
            Like.Create(alice.Id,  carolTweets[2].Id),

            // bob follows alice and dave
            Like.Create(bob.Id,    aliceTweets[0].Id),
            Like.Create(bob.Id,    aliceTweets[3].Id),
            Like.Create(bob.Id,    daveTweets[0].Id),

            // carol follows alice and eve
            Like.Create(carol.Id,  aliceTweets[1].Id),
            Like.Create(carol.Id,  eveTweets[0].Id),
            Like.Create(carol.Id,  eveTweets[3].Id),

            // dave follows bob and frank
            Like.Create(dave.Id,   bobTweets[2].Id),
            Like.Create(dave.Id,   frankTweets[0].Id),
            Like.Create(dave.Id,   frankTweets[2].Id),

            // eve follows carol and frank
            Like.Create(eve.Id,    carolTweets[1].Id),
            Like.Create(eve.Id,    carolTweets[3].Id),
            Like.Create(eve.Id,    frankTweets[1].Id),

            // frank follows grace and eve
            Like.Create(frank.Id,  graceTweets[0].Id),
            Like.Create(frank.Id,  graceTweets[2].Id),
            Like.Create(frank.Id,  eveTweets[1].Id),

            // grace follows henry and alice
            Like.Create(grace.Id,  henryTweets[0].Id),
            Like.Create(grace.Id,  aliceTweets[2].Id),
            Like.Create(grace.Id,  aliceTweets[0].Id),

            // henry follows iris and grace
            Like.Create(henry.Id,  irisTweets[0].Id),
            Like.Create(henry.Id,  irisTweets[2].Id),
            Like.Create(henry.Id,  graceTweets[3].Id),

            // iris follows jack and henry
            Like.Create(iris.Id,   jackTweets[0].Id),
            Like.Create(iris.Id,   jackTweets[2].Id),
            Like.Create(iris.Id,   henryTweets[1].Id),

            // jack follows alice and iris
            Like.Create(jack.Id,   aliceTweets[0].Id),
            Like.Create(jack.Id,   irisTweets[3].Id),
            Like.Create(jack.Id,   irisTweets[1].Id)
        );

        await db.SaveChangesAsync(ct);
    }
}
