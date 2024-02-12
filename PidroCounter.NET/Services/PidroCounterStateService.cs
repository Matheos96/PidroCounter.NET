using System.Text.Json;
using PidroCounter.NET.Shared;
using PidroCounter.NET.Shared.Utils;

namespace PidroCounter.NET.Services;

internal sealed class PidroCounterStateService : IDisposable
{
    #region Private members
    private readonly Team[] _teams = new Team[2];
    #endregion

    internal ScoreMode ScoreMode { get; set; }
    internal event Action? OnStateChanged;

    public PidroCounterStateService()
    {
        OnStateChanged += SaveState;
        RestoreState();
    }

    public void Dispose() => OnStateChanged -= SaveState; // May be rather redundant but oh well...

    public bool IsUnclearSituation(int teamId)
        => ScoreMode == ScoreMode.Reverse
        && GetOtherTeam(teamId).ScoreHistory.TryPeek(out var lastOther)
        && lastOther == 14;

    #region Team manipulation and fetching
    private static int nextId = 0;
    internal int RegisterTeam(string name)
    {
        var id = _teams.Count(t => t is not null);
        if (id == 2) return nextId++; // When existing teams are registered through RestoreState

        _teams[id] = new Team(name);
        return id;
    }

    internal ReadOnlyTeam GetTeam(int teamId) => _teams[teamId];

    internal void SetTeamName(int teamId, string newName)
    {
        _teams[teamId].Name = newName;
        OnStateChanged?.Invoke();
    }
    #endregion

    #region Score manipulation and fetching
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
        OnStateChanged?.Invoke();
        SaveState();
        return true;
    }

    public void ResetScores()
    {
        foreach (var team in _teams) team.ScoreHistory.Clear();
        OnStateChanged?.Invoke();
    }
    #endregion

    #region Save/Restore State + Serialization
    private void SaveState()
    {
        if (_teams.Length != 2) return;

        Ls.SetItem(Constants.LocalStorageKey, new PidroState
        {
            Team1 = _teams[0],
            Team2 = _teams[1]
        });
    }

    private void RestoreState()
    {
        var state = Ls.GetItem<PidroState>(Constants.LocalStorageKey);
        if (state is null) return;

        if (state?.Team1 is not null) _teams[0] = state.Team1;
        if (state?.Team2 is not null) _teams[1] = state.Team2;
    }

    private class PidroState
    {
        public Team? Team1 { get; set; }
        public Team? Team2 { get; set; }
    }
    #endregion

    #region Private helper methods
    private static bool IsLegalScore(int score) => score >= -14 && score <= 14;
    private Team GetOtherTeam(int currentTeamId) => _teams[currentTeamId ^ 1];
    #endregion

    #region Private Constants
    private static class Constants
    {
        public const string LocalStorageKey = "pidro-state";

        // Messages
        public const string IllegalScore = "The score to be added must be in the range 0 <= score <= 14!";
        public const string NotInTurn = "It's not this team's turn!";
        public const string NotPossible = "Considering the other team's previous score, that is not possible...";
        public const string BothCannotReverse = "Both teams can not reverse on the same round...";
    }
    #endregion
}
