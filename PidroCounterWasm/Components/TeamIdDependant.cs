using Microsoft.AspNetCore.Components;
using PidroCounterWasm.Services;

namespace PidroCounterWasm.Components;

public abstract class TeamIdDependant : ComponentBase
{
    [CascadingParameter, EditorRequired] public int? TeamId { get; set; }

    protected ReadOnlyTeam? Team { get; set; }

    protected override void OnInitialized()
    {
        if (!TeamId.HasValue) throw new ArgumentException($"{GetType()} NEEDS a TeamId parameter!");
    }
}
