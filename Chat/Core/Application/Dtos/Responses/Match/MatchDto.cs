namespace Application.Dtos.Responses.Match;

public class MatchDto
{
    public string MatchId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Guid GameTypeId { get; set; }
    public string GameTypeName { get; set; } = string.Empty;
    public Guid StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string TournamentId { get; set; } = string.Empty;
    public string TournamentName { get; set; } = string.Empty;
    public string WinnerTeamId { get; set; } = string.Empty;
    public string Team1Name { get; set; } = string.Empty;
    public string Team2Name { get; set; } = string.Empty;
    public string Team1Id { get; set; } = string.Empty;
    public string Team2Id { get; set; } = string.Empty;
    public bool IsWin { get; set; }
    public string PlayerTeamId { get; set; } = string.Empty;
    public string MapName { get; set; } = string.Empty;
    public string GameMode { get; set; } = string.Empty;
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int Team1Score { get; set; }
    public int Team2Score { get; set; }
}
