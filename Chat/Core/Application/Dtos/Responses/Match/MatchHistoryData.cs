using Application.Requests.Queries.Friends;

namespace Application.Dtos.Responses.Match;

public class MatchHistoryData
{
    public int TotalCount { get; set; }
    public List<MatchDto> Data { get; set; } = [];
}
