using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace SimpleChat.Client.Blazor.Pages;

public partial class Button
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public EventCallback OnClick { get; set; }

    [Parameter]
    public bool Enabled { get; set; } = true;

    private bool IsBusy { get; set; }

    private async Task HandleClickAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        await OnClick.InvokeAsync();
        IsBusy = false;
    }
}