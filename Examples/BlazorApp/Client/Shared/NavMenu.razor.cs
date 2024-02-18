namespace BlazorApp.Client.Shared;

public partial class NavMenu
{
    private bool _collapseNavMenu = true;

    private string NavMenuCssClass => _collapseNavMenu ? "collapse" : null;

    private void ToggleNavMenu() => _collapseNavMenu = !_collapseNavMenu;
}