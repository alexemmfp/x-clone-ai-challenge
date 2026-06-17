using FluentValidation;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Profile.Dtos;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Application.Profile.Commands;

public sealed record UpdateProfileCommand(Guid UserId, string? Bio, string? AvatarUrl);

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.Bio).MaximumLength(160).When(x => x.Bio is not null);
        RuleFor(x => x.AvatarUrl).MaximumLength(512).When(x => x.AvatarUrl is not null);
    }
}

public sealed class UpdateProfileHandler(IUserRepository users, IFollowRepository follows, IUnitOfWork uow)
{
    public async Task<ProfileDto> HandleAsync(UpdateProfileCommand cmd, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(cmd.UserId, ct)
            ?? throw new DomainException("User not found.");

        user.UpdateProfile(cmd.Bio, cmd.AvatarUrl);
        await uow.SaveChangesAsync(ct);

        var followerCount = await follows.CountFollowersAsync(user.Id, ct);
        var followingCount = await follows.CountFollowingAsync(user.Id, ct);

        return new ProfileDto(user.Id, user.Username, user.Email, user.Bio, user.AvatarUrl, followerCount, followingCount, false);
    }
}
