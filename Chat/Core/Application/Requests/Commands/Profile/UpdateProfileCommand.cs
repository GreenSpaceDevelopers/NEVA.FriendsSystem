using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Messaging;
using Domain.Models.Users;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Requests.Commands.Profile;

public record UpdateProfileRequest(
    Guid UserId,
    string Username,
    string? PersonalLink,
    string? Name,
    string? Surname,
    string? MiddleName,
    DateTime? DateOfBirth,
    IFormFile? Avatar,
    IFormFile? Cover)
    : IRequest;

public class UpdateProfileRequestHandler(
    IChatUsersRepository chatUsersRepository,
    IFilesStorage filesStorage,
    IFilesValidator filesValidator,
    IAttachmentsRepository attachments) : IRequestHandler<UpdateProfileRequest>
{
    public async Task<IOperationResult> HandleAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        if (user.Username != request.Username)
        {
            var isUnique = await chatUsersRepository.IsUsernameUniqueAsync(request.Username, cancellationToken);
            if (!isUnique)
            {
                return ResultsHelper.BadRequest("Username is already taken");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.PersonalLink) && user.PersonalLink != request.PersonalLink)
        {
            var isPersonalLinkUnique = await chatUsersRepository.IsPersonalLinkUniqueAsync(request.PersonalLink!, cancellationToken);
            if (!isPersonalLinkUnique)
            {
                return ResultsHelper.BadRequest("Personal link is already taken");
            }
        }

        if (request.Avatar is not null)
        {
            using var avatarStream = new MemoryStream();
            await request.Avatar.CopyToAsync(avatarStream, cancellationToken);

            if (filesValidator.ValidateFile(avatarStream, request.Avatar.FileName) is not true)
            {
                return ResultsHelper.BadRequest("Invalid avatar file");
            }

            var avatarUploadResult = await filesStorage.UploadAsync(avatarStream, request.Avatar.FileName, cancellationToken);

            if (!avatarUploadResult.IsSuccess)
            {
                return ResultsHelper.BadRequest("file upload failed");
            }

            var avatarType = await attachments.GetAttachmentTypeAsync(AttachmentTypes.Image, cancellationToken);

            var avatarAttachment = new Attachment
            {
                Id = Guid.NewGuid(),
                Url = avatarUploadResult.GetValue<string>(),
                Type = avatarType,
                TypeId = avatarType.Id,
            };

            await attachments.AddAsync(avatarAttachment, cancellationToken);
            user.Avatar = avatarAttachment;
        }

        if (request.Cover is not null)
        {
            using var coverStream = new MemoryStream();
            await request.Cover.CopyToAsync(coverStream, cancellationToken);

            if (filesValidator.ValidateFile(coverStream, request.Cover.FileName) is not true)
            {
                return ResultsHelper.BadRequest("Invalid cover file");
            }

            var coverUploadResult = await filesStorage.UploadAsync(coverStream, request.Cover.FileName, cancellationToken);

            if (!coverUploadResult.IsSuccess)
            {
                return ResultsHelper.BadRequest(coverUploadResult.GetValue<string>());
            }

            var coverType = await attachments.GetAttachmentTypeAsync(AttachmentTypes.Image, cancellationToken);

            var coverAttachment = new Attachment
            {
                Id = Guid.NewGuid(),
                Url = coverUploadResult.GetValue<string>(),
                Type = coverType,
                TypeId = coverType.Id,
            };

            await attachments.AddAsync(coverAttachment, cancellationToken);
            user.Cover = coverAttachment;
        }

        user.Username = request.Username;
        if (!string.IsNullOrWhiteSpace(request.PersonalLink))
        {
            user.PersonalLink = request.PersonalLink;
        }
        user.Name = request.Name;
        user.Surname = request.Surname;
        user.MiddleName = request.MiddleName;
        user.DateOfBirth = request.DateOfBirth?.ToUniversalTime();

        await chatUsersRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.NoContent();
    }
}

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .Must(id => id != Guid.Empty).WithMessage("UserId must not be empty.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters.");

        When(x => !string.IsNullOrWhiteSpace(x.PersonalLink), () =>
        {
            RuleFor(x => x.PersonalLink!)
                .MinimumLength(3).WithMessage("Personal link must be at least 3 characters long.")
                .MaximumLength(50).WithMessage("Personal link must not exceed 50 characters.")
                .Matches("^[a-zA-Z0-9_]+$").WithMessage("Personal link can only contain letters, numbers, and underscores.");
        });

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Surname)
            .MaximumLength(100).WithMessage("Surname must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Surname));

        RuleFor(x => x.MiddleName)
            .MaximumLength(100).WithMessage("MiddleName must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.MiddleName));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past.")
            .GreaterThan(DateTime.Today.AddYears(-150)).WithMessage("Date of birth cannot be more than 150 years ago.")
            .When(x => x.DateOfBirth.HasValue);
    }
}