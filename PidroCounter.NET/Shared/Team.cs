namespace PidroCounter.NET.Shared;

internal class ReadOnlyTeam(Team team)
{
    private readonly Team _team = team;
    internal string Name => _team.Name;
    internal int Score => _team.Score;
    internal IReadOnlyList<int> ScoreHistory => _team.ScoreHistory.Reverse().ToList();
}

internal record Team(string Name)
{
    public string Name { get; set; } = Name;
    public Stack<int> ScoreHistory { get; set; } = new();
    internal int Score => ScoreHistory.Sum();
    internal void AddScore(int score) => ScoreHistory.Push(score);

    public static implicit operator ReadOnlyTeam(Team team) => new(team);
}