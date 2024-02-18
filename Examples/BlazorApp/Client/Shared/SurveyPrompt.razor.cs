using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Shared;

public partial class SurveyPrompt
{
    // Demonstrates how a parent component can supply parameters
    [Parameter]
    [EditorRequired]
    public string Title { get; set; } = null!;
}