using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Dtos.Requests.Shared;

namespace Application.Requests.Queries.Media;

public record GetAllStickersRequest(PageSettings PageSettings) : IRequest;

