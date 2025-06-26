using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Requests.Commands.Profile;
using Application.Requests.Queries.Profile;
using Domain.Models.Users;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Mappers;
using External.WebApi.Common.Helpers;

namespace WebApi.Endpoints;

public static class Profile
{
    public static void MapProfileEndpoints(this WebApplication app)
    {
        app.MapGet("/profile/{userId:guid}", async (
            [FromRoute] Guid userId,
            [FromQuery] Guid currentUserId,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetUserProfileQuery(userId, currentUserId);
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetUserProfile");

        app.MapPut("/profile/", async (
            [FromForm] UpdateProfileForm form,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var request = new UpdateProfileRequest(form.UserId, form.Username, form.Avatar, form.Cover, form.PrivacySetting);
            var result = await sender.SendAsync(request, cancellationToken);
            return result.ToApiResult();
        })
        .DisableAntiforgery()
        .WithName("UpdateProfile");

        app.MapPost("/profile/validate-username", async (
            [FromBody] ValidateUsernameRequest request,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(request, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("ValidateUsername");
    }

    public class UpdateProfileForm
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public IFormFile? Avatar { get; set; }
        public IFormFile? Cover { get; set; }
        public PrivacySettingsEnums PrivacySetting { get; set; }
    }
} 