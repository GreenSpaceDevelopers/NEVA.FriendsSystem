using Application.Requests.Queries.Friends;

namespace Application.Dtos.Responses.Match;

public class MatchHistoryResponse
{
    public bool IsSuccess { get; set; }
    public MatchHistoryData? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
}
