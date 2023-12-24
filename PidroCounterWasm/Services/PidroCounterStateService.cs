namespace PidroCounterWasm.Services;

internal sealed class PidroCounterStateService
{
    private readonly Team[] _teams = new Team[2];

    internal int RegisterTeam(string name)
    {
        var id = _teams.Count(t => t is not null);
        _teams[id] = new Team(name);
        return id;
    }

    internal ReadOnlyTeam GetTeam(int teamId) => _teams[teamId];
    internal int GetScore(int teamId) => _teams[teamId].Score;

    internal bool TryAddScore(int teamId, int score)
    {
        if (!IsLegalScore(score)) return false;

        var team = _teams[teamId];
        var other = GetOtherTeam(teamId);
        if ((team.ScoreHistory.Count != other.ScoreHistory.Count - 1) && (team.ScoreHistory.Count != other.ScoreHistory.Count)) return false;
        if ((team.ScoreHistory.Count == other.ScoreHistory.Count - 1) && other.ScoreHistory.TryPeek(out var lastOther) && score >= 0 && lastOther + score != 14) return false;

        team.AddScore(score);
        OnScoreChange?.Invoke();
        return true;
    }

    internal event Action? OnScoreChange;


    private static bool IsLegalScore(int score) => score >= -14 && score <= 14;
    private Team GetOtherTeam(int currentTeamId) => _teams[currentTeamId ^ 1];
}

public class ReadOnlyTeam(Team team)
{
    private readonly Team _team = team;

    internal string Name => _team.Name;
    internal int Score => _team.Score;
    internal IReadOnlyList<int> ScoreHistory => _team.ScoreHistory.ToList();
}

public record Team(string Name)
{
    internal Stack<int> ScoreHistory { get; } = new();
    internal int Score => ScoreHistory.Sum();
    internal void AddScore(int score) => ScoreHistory.Push(score);

    public static implicit operator ReadOnlyTeam(Team team) => new(team);
}


