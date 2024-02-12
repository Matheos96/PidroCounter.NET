using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;

namespace PidroCounter.NET.Shared.Utils;
internal static partial class Ls
{
    internal static void SetItem<T>(string key, T value) => SetItem(key, JsonSerializer.Serialize(value));

    [JSImport("globalThis.localStorage.setItem")]
    internal static partial void SetItem(string key, string value);

    internal static T? GetItem<T>(string key) => JsonSerializer.Deserialize<T>(GetItem(key));

    [JSImport("globalThis.localStorage.getItem")]
    internal static partial string GetItem(string key);
}
