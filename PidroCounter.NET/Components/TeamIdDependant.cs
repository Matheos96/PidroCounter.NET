using Microsoft.AspNetCore.Components;
using PidroCounter.NET.Shared;

namespace PidroCounter.NET.Components;

public abstract class TeamIdDependant : ComponentBase
{
    [CascadingParameter, EditorRequired] public int? TeamId { get; set; }

    private protected ReadOnlyTeam? Team { get; set; }

    protected override void OnInitialized()
    {
        if (!TeamId.HasValue) throw new ArgumentException($"{GetType()} NEEDS a TeamId parameter!");
    }
}
