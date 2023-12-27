using System.Text.Json;
using Microsoft.JSInterop;
using PidroCounter.NET.Extensions;

namespace PidroCounter.NET.Services;

internal sealed class PidroCounterStateService
{
    private readonly IJSInProcessRuntime _js;
    private readonly Team[] _teams = new Team[2];

    internal ScoreMode ScoreMode { get; set; }

    public PidroCounterStateService(IJSRuntime jSRuntime)
    {
        _js = (IJSInProcessRuntime)jSRuntime;
        OnScoreChanged += SaveState;
        RestoreState();
    }

    private static int nextId = 0;
    internal int RegisterTeam(string name)
    {
        var id = _teams.Count(t => t is not null);
        if (id == 2) return nextId++; // When existing teams are registered through deserialization

        _teams[id] = new Team(name);
        return id;
    }

    internal ReadOnlyTeam GetTeam(int teamId) => _teams[teamId];
    internal int GetScore(int teamId) => _teams[teamId].Score;

    internal bool TryAddScore(int teamId, int score, out string warningMessage, bool addZeroes = true)
    {
        warningMessage = string.Empty;
        score = ScoreMode == ScoreMode.Normal ? score : -score;
        if (!IsLegalScore(score))
        {
            warningMessage = Constants.IllegalScore;
            return false;
        }

        var team = _teams[teamId];
        var other = GetOtherTeam(teamId);
        if (addZeroes)
        {
            if (other.ScoreHistory.Count - 1 == team.ScoreHistory.Count && other.ScoreHistory.TryPeek(out int last) && last == 14)
            {
                team.AddScore(0); // Last round's score
            }

            if (team.ScoreHistory.Count - 1 == other.ScoreHistory.Count && team.ScoreHistory.TryPeek(out last) && last == 14)
            {
                other.AddScore(0); // Last round's score
            }
        }

        if ((team.ScoreHistory.Count != other.ScoreHistory.Count - 1) && (team.ScoreHistory.Count != other.ScoreHistory.Count))
        {
            warningMessage = Constants.NotInTurn;
            return false;
        }
        if (team.ScoreHistory.Count == other.ScoreHistory.Count - 1)
        {
            if (score < 0 && other.ScoreHistory.TryPeek(out var last) && last < 0)
            {
                warningMessage = Constants.BothCannotReverse;
                return false;
            }
            if (other.ScoreHistory.TryPeek(out last) && score >= 0 && last >= 0 && last + score != 14)
            {
                warningMessage = Constants.NotPossible;
                return false;
            }
        }

        team.AddScore(score);
        OnScoreChanged?.Invoke();
        SaveState();
        return true;
    }

    public bool IsUnclearSituation(int teamId)
        => ScoreMode == ScoreMode.Revese && GetOtherTeam(teamId).ScoreHistory.TryPeek(out var lastOther) && lastOther == 14;
    public void ResetScores()
    {
        foreach (var team in _teams) team.ScoreHistory.Clear();
        OnScoreChanged?.Invoke();
    }

    internal event Action? OnScoreChanged;

    private static bool IsLegalScore(int score) => score >= -14 && score <= 14;
    private Team GetOtherTeam(int currentTeamId) => _teams[currentTeamId ^ 1];

    private void SaveState()
    {
        if (_teams.Length != 2) return;

        var json = JsonSerializer.Serialize(new PidroState
        {
            Team1 = _teams[0],
            Team2 = _teams[1]
        });
        _js.SetLocalStorage(Constants.LsKey, json);
    }

    private void RestoreState()
    {
        var json = _js.GetLocalStorage(Constants.LsKey);
        if (json is null) return;

        var state = JsonSerializer.Deserialize<PidroState>(json);
        if (state?.Team1 is not null) _teams[0] = state.Team1;
        if (state?.Team2 is not null) _teams[1] = state.Team2;
    }

    private class PidroState
    {
        public Team? Team1 { get; set; }
        public Team? Team2 { get; set; }
    }
    private static class Constants
    {
        public const string LsKey = "pidro-state";
        public const string IllegalScore = "The score to be added must be in the range 0 <= score <= 14!";
        public const string NotInTurn = "It's not this team's turn!";
        public const string NotPossible = "Considering the other team's previous score, that is not possible...";
        public const string BothCannotReverse = "Both teams can not reverse on the same round...";
    }
}

public class ReadOnlyTeam(Team team)
{
    private readonly Team _team = team;

    internal string Name => _team.Name;
    internal int Score => _team.Score;
    internal IReadOnlyList<int> ScoreHistory => _team.ScoreHistory.Reverse().ToList();
}

public record Team(string Name)
{
    public Stack<int> ScoreHistory { get; set; } = new();
    internal int Score => ScoreHistory.Sum();
    internal void AddScore(int score) => ScoreHistory.Push(score);

    public static implicit operator ReadOnlyTeam(Team team) => new(team);
}

public enum ScoreMode { Normal, Revese }
