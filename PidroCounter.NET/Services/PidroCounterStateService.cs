﻿namespace PidroCounter.NET.Services;

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

    internal bool TryAddScore(int teamId, int score, out string warningMessage)
    {
        warningMessage = string.Empty;
        if (!IsLegalScore(score))
        {
            warningMessage = Constants.IllegalScore;
            return false;
        }

        var team = _teams[teamId];
        var other = GetOtherTeam(teamId);
        if (other.ScoreHistory.Count - 1 == team.ScoreHistory.Count && other.ScoreHistory.TryPeek(out int last) && last == 14)
        {
            team.AddScore(0); // Last round's score
        }

        if (team.ScoreHistory.Count - 1 == other.ScoreHistory.Count && team.ScoreHistory.TryPeek(out last) && last == 14)
        {
            other.AddScore(0); // Last round's score
        }

        if ((team.ScoreHistory.Count != other.ScoreHistory.Count - 1) && (team.ScoreHistory.Count != other.ScoreHistory.Count))
        {
            warningMessage = Constants.NotInTurn;
            return false;
        }
        if ((team.ScoreHistory.Count == other.ScoreHistory.Count - 1) && other.ScoreHistory.TryPeek(out last) && score >= 0 && last + score != 14)
        {
            warningMessage = Constants.NotPossible;
            return false;
        }

        team.AddScore(score);
        OnScoreChange?.Invoke();
        return true;
    }

    internal event Action? OnScoreChange;


    private static bool IsLegalScore(int score) => score >= -14 && score <= 14;
    private Team GetOtherTeam(int currentTeamId) => _teams[currentTeamId ^ 1];

    private static class Constants
    {
        public const string IllegalScore = "The score to be added must be in the range 0 <= score <= 14!";
        public const string NotInTurn = "It's not this team's turn!";
        public const string NotPossible = "Considering the other team's previous score, that is not possible...";
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
    internal Stack<int> ScoreHistory { get; } = new();
    internal int Score => ScoreHistory.Sum();
    internal void AddScore(int score) => ScoreHistory.Push(score);

    public static implicit operator ReadOnlyTeam(Team team) => new(team);
}


