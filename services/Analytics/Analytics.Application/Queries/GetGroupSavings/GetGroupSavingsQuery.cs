using Analytics.Application.DTOs;
using Analytics.Domain.ValueObjects;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Application.Queries.GetGroupSavings;

public record GetGroupSavingsQuery(
    GroupId GroupId,
    int Year) : IRequest<Result<GroupSavingsDto>>;
