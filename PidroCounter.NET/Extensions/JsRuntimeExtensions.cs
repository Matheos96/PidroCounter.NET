namespace PidroCounter.NET.Extensions;

using Microsoft.JSInterop;

public static class JsRuntimeExtensions
{
    internal static void SetLocalStorage(this IJSRuntime js, string key, string value) => ((IJSInProcessRuntime)js).InvokeVoid("localStorage.setItem", key, value);
    internal static string GetLocalStorage(this IJSRuntime js, string key) => ((IJSInProcessRuntime)js).Invoke<string>("localStorage.getItem", key);
}
